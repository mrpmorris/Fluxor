using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Fluxor;
using Fluxor.DependencyInjection.Generated;
using Microsoft.Extensions.DependencyInjection;

// Fluxor.Performance.Runner <sample> [--dispatches N --batches B]
//   <sample>: [small-|medium-|complex-]<generated|scanned>, or scaling-<name>-<generated|scanned> (a ScalingSample)
// Measures one cold start of Fluxor with the chosen sample, in this process, once (nothing is warmed up),
// and prints the result as one JSON line. Run it as a new process for every measurement. Without a size it uses the
// standard sample; with one, the browser benchmark's sample of that size. With --dispatches, it then also times warm
// dispatches of the first action: N warm-up dispatches, then B batches of N.
const string Usage =
	"Usage: Fluxor.Performance.Runner <[small-|medium-|complex-]<generated|scanned> | scaling-<name>-<generated|scanned>>"
	+ " [--dispatches N --batches B]";
if (args.Length is not (1 or 5) || !TryGetSampleAssembly(args[0], out string? assemblyName))
{
	Console.Error.WriteLine(Usage);
	return 1;
}
int dispatchesPerBatch = 0;
int dispatchBatches = 0;
if (args.Length == 5
	&& (args[1] != "--dispatches" || !int.TryParse(args[2], CultureInfo.InvariantCulture, out dispatchesPerBatch) || dispatchesPerBatch < 1
		|| args[3] != "--batches" || !int.TryParse(args[4], CultureInfo.InvariantCulture, out dispatchBatches) || dispatchBatches < 1))
{
	Console.Error.WriteLine(Usage);
	return 1;
}

// Loading the sample is not measured: an app's own assembly is loaded before it calls AddFluxor
Assembly sampleAssembly = Assembly.Load(assemblyName);
// By name, not by listing the types: that would warm the reflection the scan is measured with. Samples with
// 1,000 states or more number them with 4 digits.
Type firstActionType =
	sampleAssembly.GetType("Fluxor.Performance.Sample.State000Action0")
	?? sampleAssembly.GetType("Fluxor.Performance.Sample.State0000Action0", throwOnError: true)!;
object firstAction = Activator.CreateInstance(firstActionType, 1)!;

var stopwatch = new Stopwatch();
var jit = new JitCounter();

jit.StartPhase();
stopwatch.Restart();
var services = new ServiceCollection();
services.AddFluxor(options => options.ScanAssemblies(sampleAssembly));
double addFluxor = stopwatch.Elapsed.TotalMilliseconds;
jit.EndPhase("addFluxor");

jit.StartPhase();
stopwatch.Restart();
ServiceProvider serviceProvider = services.BuildServiceProvider();
double buildProvider = stopwatch.Elapsed.TotalMilliseconds;
jit.EndPhase("buildProvider");

jit.StartPhase();
stopwatch.Restart();
IStore store = serviceProvider.GetRequiredService<IStore>();
double resolveStore = stopwatch.Elapsed.TotalMilliseconds;
jit.EndPhase("resolveStore");

jit.StartPhase();
stopwatch.Restart();
await store.InitializeAsync();
double initialize = stopwatch.Elapsed.TotalMilliseconds;
jit.EndPhase("initialize");

IDispatcher dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
jit.StartPhase();
stopwatch.Restart();
dispatcher.Dispatch(firstAction);
double firstDispatch = stopwatch.Elapsed.TotalMilliseconds;
jit.EndPhase("firstDispatch");

// Warm dispatches, after the store is built and the first dispatch compiled the dispatch path
string dispatchJson = "";
if (dispatchesPerBatch > 0)
{
	for (int index = 0; index < dispatchesPerBatch; index++)
		dispatcher.Dispatch(firstAction);
	var batchMicroseconds = new List<double>();
	for (int batch = 0; batch < dispatchBatches; batch++)
	{
		stopwatch.Restart();
		for (int index = 0; index < dispatchesPerBatch; index++)
			dispatcher.Dispatch(firstAction);
		batchMicroseconds.Add(stopwatch.Elapsed.TotalMicroseconds / dispatchesPerBatch);
	}
	int handlingReducers = store.Features.Values
		.SelectMany(x => GetField<System.Collections.IEnumerable>(x, "Reducers").Cast<object>())
		.Count(x => ShouldReduce(x, firstAction));
	int reactingEffects = GetField<System.Collections.IEnumerable>(store, "Effects").Cast<IEffect>()
		.Count(x => x.ShouldReactToAction(firstAction));
	dispatchJson =
		$",\"dispatch\":{{\"dispatchesPerBatch\":{dispatchesPerBatch},"
		+ $"\"batchMicroseconds\":[{string.Join(",", batchMicroseconds.Select(F))}],"
		+ $"\"reducers\":{handlingReducers},\"effects\":{reactingEffects}}}";
}

// The store's shape, so a variant cannot look faster by registering less
int features = store.Features.Count;
int reducers = store.Features.Values.Sum(CountReducers);
int effects = CountEffects(store);
FluxorGeneratedRegistrationAttribute? generatedRegistration = sampleAssembly.GetCustomAttribute<FluxorGeneratedRegistrationAttribute>();
bool hasGeneratedRegistration = generatedRegistration is not null;
int generatedMethods = generatedRegistration is null ? 0 : CountMethods(generatedRegistration.RegistrationType);
// Which path built the store: wrappers created from generated code hold the generated delegate in Handler
int generatedReducers = store.Features.Values
	.SelectMany(x => GetField<System.Collections.IEnumerable>(x, "Reducers").Cast<object>())
	.Count(HasGeneratedHandler);
int generatedEffects = GetField<System.Collections.IEnumerable>(store, "Effects").Cast<object>().Count(HasGeneratedHandler);
// T and M: the types of the sample assembly and their public methods, inherited ones included (what the scan reads)
Type[] sampleTypes = sampleAssembly.GetTypes();
int sampleMethods = sampleTypes.Sum(x =>
	x.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy).Length);

Console.WriteLine(
	"{"
	+ $"\"variant\":\"{args[0]}\","
	+ $"\"phases\":{{\"addFluxor\":{F(addFluxor)},\"buildProvider\":{F(buildProvider)},\"resolveStore\":{F(resolveStore)},"
	+ $"\"initialize\":{F(initialize)},\"firstDispatch\":{F(firstDispatch)}}},"
	+ $"\"features\":{features},\"reducers\":{reducers},\"effects\":{effects},"
	+ $"\"hasGeneratedRegistration\":{(hasGeneratedRegistration ? "true" : "false")},"
	+ $"\"generatedMethods\":{generatedMethods},"
	+ $"\"generatedReducers\":{generatedReducers},\"generatedEffects\":{generatedEffects},"
	+ $"\"types\":{sampleTypes.Length},\"methods\":{sampleMethods},"
	+ $"\"jit\":{jit.ToJson()}"
	+ dispatchJson
	+ "}");

await serviceProvider.DisposeAsync();
return 0;

// Methods (and constructors) of the generated registration, lambdas included (the compiler puts them in nested types)
static int CountMethods(Type type)
{
	const BindingFlags declared =
		BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
	return type.GetMethods(declared).Length
		+ type.GetConstructors(declared).Length
		+ type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic).Sum(CountMethods);
}

static string F(double value) => value.ToString("0.000", CultureInfo.InvariantCulture);

// Reducers are IReducer<TState>; ask the one of the reducer's state type
static bool ShouldReduce(object reducer, object action)
{
	Type reducerInterface = reducer.GetType().GetInterfaces()
		.Single(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IReducer<>));
	return (bool)reducerInterface.GetMethod(nameof(IReducer<object>.ShouldReduceStateForAction))!.Invoke(reducer, [action])!;
}

static bool HasGeneratedHandler(object wrapper)
{
	FieldInfo? handler = wrapper.GetType().GetField("Handler", BindingFlags.Instance | BindingFlags.NonPublic);
	return handler?.GetValue(wrapper) is not null;
}

static int CountReducers(IFeature feature) =>
	GetField<System.Collections.ICollection>(feature, "Reducers").Count;

static int CountEffects(IStore store) =>
	GetField<System.Collections.ICollection>(store, "Effects").Count;

// Fluxor keeps reducers and effects in non-public lists; read them only after everything is measured
static T GetField<T>(object instance, string name)
{
	for (Type? type = instance.GetType(); type is not null; type = type.BaseType)
	{
		FieldInfo? field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
		if (field is not null)
			return (T)field.GetValue(instance)!;
	}
	throw new MissingFieldException(instance.GetType().FullName, name);
}

/// <summary>
/// Methods the JIT compiled, and the time it took, in each phase: shows how much of a phase is JIT.
/// </summary>
internal sealed class JitCounter
{
	private readonly List<string> Phases = new();
	private long StartMethods;
	private TimeSpan StartTime;

	public void StartPhase()
	{
		StartMethods = System.Runtime.JitInfo.GetCompiledMethodCount();
		StartTime = System.Runtime.JitInfo.GetCompilationTime();
	}

	public void EndPhase(string name)
	{
		long methods = System.Runtime.JitInfo.GetCompiledMethodCount() - StartMethods;
		double milliseconds = (System.Runtime.JitInfo.GetCompilationTime() - StartTime).TotalMilliseconds;
		Phases.Add(
			$"\"{name}\":{{\"methods\":{methods},\"ms\":{milliseconds.ToString("0.000", CultureInfo.InvariantCulture)}}}");
	}

	public string ToJson() => "{" + string.Join(",", Phases) + "}";
}

internal static partial class Program
{
	// Before SampleAssemblies, which is made from it
	private static readonly Dictionary<string, string> Builds = new(StringComparer.OrdinalIgnoreCase)
	{
		["generated"] = "Generated",
		["scanned"] = "Scanned"
	};

	private static readonly Dictionary<string, string> SampleAssemblies = CreateSampleAssemblies();

	private static bool TryGetSampleAssembly(string sample, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? assemblyName)
	{
		if (SampleAssemblies.TryGetValue(sample, out assemblyName))
			return true;

		// scaling-<name>-<generated|scanned>: the sample's project and assembly are named after it
		string[] parts = sample.Split('-');
		if (parts.Length == 3 && parts[0] == "scaling" && parts[1].Length > 0 && Builds.TryGetValue(parts[2], out string? build))
		{
			assemblyName = $"Fluxor.Performance.Sample.Scaling.{parts[1]}.{build}";
			return true;
		}
		assemblyName = null;
		return false;
	}

	private static Dictionary<string, string> CreateSampleAssemblies()
	{
		var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		foreach ((string name, string build) in Builds)
		{
			result[name] = "Fluxor.Performance.Sample." + build;
			foreach (string size in new[] { "Small", "Medium", "Complex" })
				result[size.ToLowerInvariant() + "-" + name] = $"Fluxor.Performance.Sample.{size}.{build}";
		}
		return result;
	}
}
