namespace Fluxor.Performance.Samples;

/// <summary>
/// Which forms of Fluxor code the sample uses.
/// </summary>
public enum SampleProfile
{
	/// <summary>
	/// Static <c>[ReducerMethod]</c>s with an action parameter, instance <c>[EffectMethod]</c>s with an action
	/// parameter, states with public constructors; no reducer or effect classes.
	/// </summary>
	Simple,

	/// <summary>
	/// The forms of the standard sample (SampleOptions.Default): static and instance hosts, methods with and without
	/// an action parameter (<c>[ReducerMethod(typeof(A))]</c>), some states with private constructors, some
	/// <c>IReducer&lt;T&gt;</c> and <c>IEffect</c> classes.
	/// </summary>
	Standard,

	/// <summary>
	/// The <see cref="Simple"/> forms, plus some states declared by a <c>Feature&lt;T&gt;</c> class
	/// (<see cref="SampleOptions.FeatureClassStates"/>) and some <c>IReducer&lt;T&gt;</c> and <c>IEffect</c> classes.
	/// </summary>
	SimpleWithClasses,

	/// <summary>
	/// The <see cref="Standard"/> forms plus every other form Fluxor registers, chosen by <c>state % 10</c>:
	/// 0 private constructor; 1 a 3-level inheritance chain whose first level (with the state and its actions) is in
	/// a separate assembly that does not opt in (overrides and <c>new</c>); 2 a public or private
	/// <c>CreateInitialStateMethodName</c> factory; 3 <c>required</c> members; 4 a <c>Feature&lt;T&gt;</c> class;
	/// 5 nested types; 6 a <c>partial</c> host across two files; 7 effects and a reducer on a base-class action and
	/// an interface action; 8 two reducers and two effects per action; 9 a plain state. Also one middleware namespace
	/// whose Fluxor code the scan leaves out. Every state has 10 reducers and 10 effects.
	/// </summary>
	Complex
}

/// <summary>
/// What the <see cref="SampleWriter"/> writes. Every option is deterministic: the same options always
/// produce the same files.
/// </summary>
public sealed record SampleOptions
{
	/// <summary>
	/// The standard sample (Fluxor.Performance.Sample.Generated and .Scanned): 200 states, 2,000 reducers, 2,000 effects
	/// over 300 classes.
	/// </summary>
	public static readonly SampleOptions Default = new();

	public SampleProfile Profile { get; init; } = SampleProfile.Standard;

	/// <summary>Number of <c>[FeatureState]</c> records.</summary>
	public int States { get; init; } = 200;

	/// <summary>Reducers per state (reducer methods plus reducer classes); one action type per reducer.</summary>
	public int ReducersPerState { get; init; } = 10;

	/// <summary>Effects per state (effect methods plus effect classes), reacting to the state's actions.</summary>
	public int EffectsPerState { get; init; } = 10;

	/// <summary>
	/// When set, the number of reducers in all, instead of <see cref="ReducersPerState"/>: every state gets the
	/// same share, and the first states one more for what does not divide evenly.
	/// </summary>
	public int? TotalReducers { get; init; }

	/// <summary>When set, the number of effects in all, instead of <see cref="EffectsPerState"/> (as <see cref="TotalReducers"/>).</summary>
	public int? TotalEffects { get; init; }

	/// <summary>States with a private parameterless constructor (Standard profile only), spread evenly.</summary>
	public int PrivateConstructorStates { get; init; } = 20;

	/// <summary>States declared by a <c>Feature&lt;T&gt;</c> class (SimpleWithClasses profile only), spread evenly.</summary>
	public int FeatureClassStates { get; init; }

	/// <summary><c>IReducer&lt;T&gt;</c> classes (not in the Simple profile), each replacing one reducer method.</summary>
	public int ReducerClasses { get; init; } = 10;

	/// <summary><c>IEffect</c> classes (not in the Simple profile), each replacing one effect method.</summary>
	public int EffectClasses { get; init; } = 10;

	/// <summary>Classes the reducer methods are spread over (only classes that get methods are written).</summary>
	public int ReducerHostClasses { get; init; } = 140;

	/// <summary>Classes the effect methods are spread over (only classes that get methods are written).</summary>
	public int EffectHostClasses { get; init; } = 140;

	/// <summary>Classes with no Fluxor code at all, to grow the code the reflection scan has to look through.</summary>
	public int PlainClasses { get; init; }

	/// <summary>Public methods on each plain class.</summary>
	public int MethodsPerPlainClass { get; init; } = 10;

	/// <summary>Namespace of the written code.</summary>
	public string Namespace { get; init; } = "Fluxor.Performance.Sample";

	/// <summary>The reducers of a state.</summary>
	public int ReducersOf(int state) => Share(TotalReducers, ReducersPerState, state);

	/// <summary>The effects of a state.</summary>
	public int EffectsOf(int state) => Share(TotalEffects, EffectsPerState, state);

	/// <summary>Throws when the options cannot be written.</summary>
	public void Validate()
	{
		RequireAtLeast(States, 1, nameof(States));
		RequireAtLeast(ReducersPerState, 1, nameof(ReducersPerState));
		RequireAtLeast(EffectsPerState, 0, nameof(EffectsPerState));
		if (TotalReducers is int totalReducers)
			RequireAtLeast(totalReducers, States, nameof(TotalReducers));
		if (TotalEffects is int totalEffects)
			RequireAtLeast(totalEffects, 0, nameof(TotalEffects));
		RequireAtLeast(ReducerHostClasses, 1, nameof(ReducerHostClasses));
		RequireAtLeast(EffectHostClasses, 1, nameof(EffectHostClasses));
		RequireAtLeast(PlainClasses, 0, nameof(PlainClasses));
		RequireAtLeast(MethodsPerPlainClass, 0, nameof(MethodsPerPlainClass));
		RequireBetween(PrivateConstructorStates, 0, States, nameof(PrivateConstructorStates));
		RequireBetween(FeatureClassStates, 0, States, nameof(FeatureClassStates));
		RequireBetween(ReducerClasses, 0, States, nameof(ReducerClasses));
		bool hasEffects = Enumerable.Range(0, States).Any(x => EffectsOf(x) > 0);
		RequireBetween(EffectClasses, 0, hasEffects ? States : 0, nameof(EffectClasses));
		if (Profile == SampleProfile.Complex)
		{
			// The forms of the Complex profile each write exactly 10 reducers and 10 effects for their state
			RequireAtLeast(States, 10, nameof(States));
			if (ReducersPerState != 10 || EffectsPerState != 10 || TotalReducers is not null || TotalEffects is not null)
				throw new ArgumentException("The Complex profile has 10 reducers and 10 effects per state");
		}
		if (string.IsNullOrWhiteSpace(Namespace))
			throw new ArgumentException("A namespace is required", nameof(Namespace));
	}

	private int Share(int? total, int perState, int state) =>
		total is int all
			? all / States + (state < all % States ? 1 : 0)
			: perState;

	private static void RequireAtLeast(int value, int minimum, string name)
	{
		if (value < minimum)
			throw new ArgumentOutOfRangeException(name, value, $"{name} must be at least {minimum}");
	}

	private static void RequireBetween(int value, int minimum, int maximum, string name)
	{
		if (value < minimum || value > maximum)
			throw new ArgumentOutOfRangeException(name, value, $"{name} must be between {minimum} and {maximum}");
	}
}

/// <summary>
/// The samples the Blazor WebAssembly benchmark compares, each compiled two ways (scanned and generated) by the
/// projects <c>Source/Performance/Samples/Fluxor.Performance.Sample.Size.Scanned|Generated</c>, which write it into
/// their obj folder at build time (<c>--size Name</c>).
/// </summary>
/// <param name="Name">small, medium or complex</param>
/// <param name="Options">What the writer writes</param>
public sealed record SizedSample(string Name, SampleOptions Options)
{
	/// <summary>20 states, 100 reducers, 100 effects; simple forms only.</summary>
	public static readonly SizedSample Small = new(
		"small",
		new SampleOptions
		{
			Profile = SampleProfile.Simple,
			States = 20,
			ReducersPerState = 5,
			EffectsPerState = 5,
			PrivateConstructorStates = 0,
			ReducerClasses = 0,
			EffectClasses = 0,
			ReducerHostClasses = 14,
			EffectHostClasses = 14
		});

	/// <summary>80 states, 600 reducers, 600 effects; simple forms plus Feature&lt;T&gt;, IReducer&lt;T&gt; and IEffect classes.</summary>
	public static readonly SizedSample Medium = new(
		"medium",
		new SampleOptions
		{
			Profile = SampleProfile.SimpleWithClasses,
			States = 80,
			TotalReducers = 600,
			TotalEffects = 600,
			PrivateConstructorStates = 0,
			FeatureClassStates = 8,
			ReducerClasses = 4,
			EffectClasses = 4,
			ReducerHostClasses = 56,
			EffectHostClasses = 56
		});

	/// <summary>200 states, 2,000 reducers, 2,000 effects; every form (see <see cref="SampleProfile.Complex"/>).</summary>
	public static readonly SizedSample Complex = new(
		"complex",
		new SampleOptions
		{
			Profile = SampleProfile.Complex,
			States = 200,
			ReducersPerState = 10,
			EffectsPerState = 10,
			ReducerClasses = 10,
			EffectClasses = 10,
			ReducerHostClasses = 70,
			EffectHostClasses = 70
		});

	public static readonly IReadOnlyList<SizedSample> All = [Small, Medium, Complex];

	public static SizedSample Get(string name) =>
		All.SingleOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase))
		?? throw new ArgumentException($"Unknown size {name}; expected one of: {string.Join(", ", All.Select(x => x.Name))}");
}
