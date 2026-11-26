using System.Text.Json;

namespace Fluxor.Performance.BlazorWasm.Benchmark;

/// <summary>What the app reported after one load, read from <c>window.fluxorBench</c>.</summary>
internal sealed record LoadResult(
	IReadOnlyDictionary<string, double> Marks,
	IReadOnlyList<LongTask> LongTasks,
	StoreShape Shape)
{
	// The marks the app sets with performance.mark, in the order it sets them
	public const string MainMark = "fluxor:main";
	public const string BeforeAddFluxorMark = "fluxor:before-add-fluxor";
	public const string AfterAddFluxorMark = "fluxor:after-add-fluxor";
	public const string AfterBuildMark = "fluxor:after-build";
	public const string StoreInitializedMark = "fluxor:store-initialized";
	public const string FirstRenderMark = "fluxor:first-render";

	/// <summary>A task of at least this long blocks the main thread (the browser's long task limit).</summary>
	private const double LongTaskMilliseconds = 50;

	/// <summary>Navigation start to <c>Main</c>: downloading and starting the runtime and the assemblies.</summary>
	public double Boot => Mark(MainMark);

	/// <summary><c>Main</c> to just before <c>AddFluxor</c>: <c>WebAssemblyHostBuilder.CreateDefault</c>.</summary>
	public double HostBuilder => Mark(BeforeAddFluxorMark) - Mark(MainMark);

	/// <summary>The <c>AddFluxor</c> call: the reflection scan, or reading the generated registration.</summary>
	public double AddFluxor => Mark(AfterAddFluxorMark) - Mark(BeforeAddFluxorMark);

	/// <summary><c>builder.Build()</c>.</summary>
	public double Build => Mark(AfterBuildMark) - Mark(AfterAddFluxorMark);

	/// <summary>
	/// After <c>Build()</c> to <c>IStore.Initialized</c> completed: starting the host, the first render of the app,
	/// creating the store with every feature, reducer and effect, and initializing it.
	/// </summary>
	public double StoreInitialized => Mark(StoreInitializedMark) - Mark(AfterBuildMark);

	/// <summary>Navigation start to the first render of the page.</summary>
	public double FirstRender => Mark(FirstRenderMark);

	/// <summary>What the speed bar is on: <c>AddFluxor</c> + store initialized.</summary>
	public double Measured => AddFluxor + StoreInitialized;

	/// <summary>Navigation start to the later of first render and store initialized.</summary>
	public double Ready => Math.Max(Mark(FirstRenderMark), Mark(StoreInitializedMark));

	/// <summary>
	/// Total blocking time up to <see cref="Ready"/>: for every long task that started before it, the part over 50 ms.
	/// </summary>
	public double TotalBlocked => TasksUntilReady.Sum(x => Math.Max(0, x.Duration - LongTaskMilliseconds));

	/// <summary>The longest task that started before <see cref="Ready"/>.</summary>
	public double LongestTask => TasksUntilReady.Select(x => x.Duration).DefaultIfEmpty(0).Max();

	private IEnumerable<LongTask> TasksUntilReady => LongTasks.Where(x => x.Start < Ready);

	private double Mark(string name) =>
		Marks.TryGetValue(name, out double value)
			? value
			: throw new InvalidOperationException($"The app did not set the mark {name}");

	public static LoadResult Parse(string json)
	{
		using JsonDocument document = JsonDocument.Parse(json);
		JsonElement root = document.RootElement;

		var marks = new Dictionary<string, double>();
		foreach (JsonProperty mark in root.GetProperty("marks").EnumerateObject())
			marks[mark.Name] = mark.Value.GetDouble();

		LongTask[] longTasks = root.GetProperty("longTasks")
			.EnumerateArray()
			.Select(x => new LongTask(x.GetProperty("start").GetDouble(), x.GetProperty("duration").GetDouble()))
			.ToArray();

		JsonElement shape = root.GetProperty("shape");
		return new LoadResult(
			marks,
			longTasks,
			new StoreShape(
				Features: shape.GetProperty("features").GetInt32(),
				Reducers: shape.GetProperty("reducers").GetInt32(),
				Effects: shape.GetProperty("effects").GetInt32(),
				MethodReducers: shape.GetProperty("methodReducers").GetInt32(),
				MethodEffects: shape.GetProperty("methodEffects").GetInt32(),
				GeneratedReducers: shape.GetProperty("generatedReducers").GetInt32(),
				GeneratedEffects: shape.GetProperty("generatedEffects").GetInt32(),
				HasGeneratedRegistration: shape.GetProperty("hasGeneratedRegistration").GetBoolean(),
				Runtime: shape.GetProperty("runtime").GetString() ?? ""));
	}
}

internal sealed record LongTask(double Start, double Duration);

/// <summary>
/// The store the app built. Method reducers/effects are the ones made from <c>[ReducerMethod]</c>/<c>[EffectMethod]</c>;
/// generated ones are those of them created by generated code (the rest come from the reflection scan).
/// </summary>
internal sealed record StoreShape(
	int Features,
	int Reducers,
	int Effects,
	int MethodReducers,
	int MethodEffects,
	int GeneratedReducers,
	int GeneratedEffects,
	bool HasGeneratedRegistration,
	string Runtime);
