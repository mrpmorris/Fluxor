namespace Fluxor.Performance.Samples;

/// <summary>
/// The samples of the scaling proofs (Fluxor.PerformanceTests ScalingTests, and the browser benchmark's non-Fluxor
/// code comparison). Like every sample, not checked in: each sample project under
/// <c>Source/Performance/Samples/Scaling</c> (<c>Fluxor.Performance.Sample.Scaling.Name.Scanned|Generated</c>) runs
/// the writer with <c>--scaling Name</c> at build time and compiles what it writes into its obj folder.
/// </summary>
/// <param name="Name">The sample's name: part of its project and assembly names</param>
/// <param name="Options">What the writer writes</param>
public sealed record ScalingSample(string Name, SampleOptions Options)
{
	/// <summary>
	/// K at 1x: the standard sample (<see cref="SampleOptions.Default"/>: 200 states, 10 reducers and 10 effects each).
	/// Big enough that the cost that grows with K is most of a cold start: at 25 states the fixed cost of a cold
	/// process (Fluxor's own JIT, the container) is most of it and would hide the order of growth.
	/// </summary>
	public static readonly ScalingSample K1 = FluxorMembers(1);
	public static readonly ScalingSample K2 = FluxorMembers(2);
	public static readonly ScalingSample K4 = FluxorMembers(4);
	public static readonly ScalingSample K8 = FluxorMembers(8);

	public static readonly ScalingSample K1Plain2000 = NonFluxorCode(2_000);
	public static readonly ScalingSample K1Plain4000 = NonFluxorCode(4_000);
	public static readonly ScalingSample K1Plain8000 = NonFluxorCode(8_000);

	/// <summary>The browser benchmark's complex sample with 4,000 plain classes.</summary>
	public static readonly ScalingSample ComplexPlain4000 = new(
		"ComplexPlain4000",
		SizedSample.Complex.Options with { PlainClasses = 4_000 });

	/// <summary>The Fluxor members (and every kind of Fluxor type) at 1x, 2x, 4x and 8x.</summary>
	public static readonly IReadOnlyList<ScalingSample> FluxorMemberSeries = [K1, K2, K4, K8];

	/// <summary>The 1x sample with 0, 2,000, 4,000 and 8,000 plain classes of 10 public methods each.</summary>
	public static readonly IReadOnlyList<ScalingSample> NonFluxorCodeSeries = [K1, K1Plain2000, K1Plain4000, K1Plain8000];

	public static readonly IReadOnlyList<ScalingSample> All = [K1, K2, K4, K8, K1Plain2000, K1Plain4000, K1Plain8000, ComplexPlain4000];

	/// <summary>F: features (every state of these samples has one).</summary>
	public int Features => Options.States;

	/// <summary>R: reducers (methods and classes).</summary>
	public int Reducers => Enumerable.Range(0, Options.States).Sum(Options.ReducersOf);

	/// <summary>E: effects (methods and classes).</summary>
	public int Effects => Enumerable.Range(0, Options.States).Sum(Options.EffectsOf);

	/// <summary>K = F + R + E (the samples have no middleware that the scan registers).</summary>
	public int Members => Features + Reducers + Effects;

	/// <summary>Public methods of the plain classes, which have no Fluxor code.</summary>
	public int AddedMethods => Options.PlainClasses * Options.MethodsPerPlainClass;

	/// <summary>The sample's assembly for a build: <c>Scanned</c> or <c>Generated</c>.</summary>
	public string AssemblyName(string build) =>
		build is "Scanned" or "Generated"
			? $"Fluxor.Performance.Sample.Scaling.{Name}.{build}"
			: throw new ArgumentException($"Unknown build {build}", nameof(build));

	/// <summary>The Fluxor.Performance.Runner argument for a variant: <c>scanned</c> or <c>generated</c>.</summary>
	public string RunnerVariant(string variant) => $"scaling-{Name}-{variant}";

	public static ScalingSample Get(string name) =>
		All.SingleOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase))
		?? throw new ArgumentException($"Unknown scaling sample {name}; expected one of: {string.Join(", ", All.Select(x => x.Name))}");

	/// <summary>
	/// The standard sample with every count multiplied: states (so features, reducers and effects), the reducer and
	/// effect host classes, the reducer and effect classes and the private-constructor states.
	/// </summary>
	private static ScalingSample FluxorMembers(int multiple)
	{
		SampleOptions one = SampleOptions.Default;
		return new ScalingSample(
			"K" + multiple,
			one with
			{
				States = one.States * multiple,
				ReducerHostClasses = one.ReducerHostClasses * multiple,
				EffectHostClasses = one.EffectHostClasses * multiple,
				ReducerClasses = one.ReducerClasses * multiple,
				EffectClasses = one.EffectClasses * multiple,
				PrivateConstructorStates = one.PrivateConstructorStates * multiple
			});
	}

	private static ScalingSample NonFluxorCode(int plainClasses) =>
		new("K1Plain" + plainClasses, K1.Options with { PlainClasses = plainClasses });
}
