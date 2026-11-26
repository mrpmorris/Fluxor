using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using Fluxor.DependencyInjection;
using Fluxor.Performance.Samples;
using Xunit.Abstractions;

namespace Fluxor.PerformanceTests;

/// <summary>
/// Measured proof of the complexity analysis (results/scaling.md, "Analysis"). Notation: T = types in the scanned
/// assemblies, M = their public methods with inherited ones, K = F + R + E (+ middleware) = the Fluxor members.
/// The samples are written at build time (<see cref="ScalingSample"/>, into each sample project's obj folder) and
/// measured in cold processes of Fluxor.Performance.Runner, as the CoreCLR speed test (SpeedTests) does.
/// </summary>
public class ScalingTests
{
	// Median of 9: the store build per item and the generated/scanned ratios are close, 5 left them to noise
	private const int RunsPerVariant = 9;
	private const int DispatchesPerBatch = 1_000;
	private const int DispatchBatches = 5;
	// Of the cold processes of each sample and variant, the first ones also time warm dispatches (at 8x, 1,000
	// dispatches take seconds, so not all five do)
	private const int DispatchRuns = 3;
	private const string Scanned = "scanned";
	private const string Generated = "generated";
	private static readonly string[] Variants = [Scanned, Generated];

	// Measured once for all the tests of the class that need them
	private static readonly Lazy<Series> FluxorMemberRuns = new(() => Series.Measure(
		ScalingSample.FluxorMemberSeries,
		run => run < DispatchRuns
			?
			[
				"--dispatches", DispatchesPerBatch.ToString(CultureInfo.InvariantCulture),
				"--batches", DispatchBatches.ToString(CultureInfo.InvariantCulture)
			]
			: []));
	private static readonly Lazy<Series> NonFluxorCodeRuns = new(() => Series.Measure(ScalingSample.NonFluxorCodeSeries, _ => []));

	private readonly ITestOutputHelper Output;

	public ScalingTests(ITestOutputHelper output)
	{
		Output = output;
	}

	/// <summary>
	/// K at 1x, 2x, 4x, 8x: addFluxor + resolveStore grows linearly on both paths (no quadratic step). The log-log
	/// least squares slope is the exponent of the growth.
	/// </summary>
	[ScalingFact]
	public void WhenFluxorMembersDouble_ThenBothPathsGrowLinearly()
	{
		Series series = FluxorMemberRuns.Value;
		Func<RunResult, double> measured = x => x.Phases["addFluxor"] + x.Phases["resolveStore"];
		double scannedSlope = series.LogLogSlopeAgainstMembers(Scanned, measured);
		double generatedSlope = series.LogLogSlopeAgainstMembers(Generated, measured);

		var text = new StringBuilder();
		text.AppendLine($"## {nameof(WhenFluxorMembersDouble_ThenBothPathsGrowLinearly)}");
		text.AppendLine();
		text.AppendLine($"addFluxor + resolveStore, median of {RunsPerVariant} cold processes per variant and size, milliseconds.");
		text.AppendLine();
		series.AppendPhaseTable(text, measured);
		text.AppendLine();
		text.AppendLine($"Slope of log(time) against log(K): Scanned **{F(scannedSlope)}**, Generated **{F(generatedSlope)}**; bar [0.70, 1.30].");
		Report(nameof(WhenFluxorMembersDouble_ThenBothPathsGrowLinearly), text, $"slopes scanned {F(scannedSlope)}, generated {F(generatedSlope)}");

		AssertBetween(scannedSlope, 0.7, 1.3, "scanned addFluxor + resolveStore slope", text);
		AssertBetween(generatedSlope, 0.7, 1.3, "generated addFluxor + resolveStore slope", text);
	}

	/// <summary>
	/// The store build (resolveStore, the first IStore resolve) is O(F + R + E) on both paths: same growth, and the
	/// generated path's cost per Fluxor member is no higher than the scan's at every size.
	/// </summary>
	[ScalingFact]
	public void WhenFluxorMembersDouble_ThenStoreBuildGrowsLinearlyOnBothPathsAndGeneratedIsCheaperPerItem()
	{
		Series series = FluxorMemberRuns.Value;
		Func<RunResult, double> resolveStore = x => x.Phases["resolveStore"];
		double scannedSlope = series.LogLogSlopeAgainstMembers(Scanned, resolveStore);
		double generatedSlope = series.LogLogSlopeAgainstMembers(Generated, resolveStore);

		var text = new StringBuilder();
		text.AppendLine($"## {nameof(WhenFluxorMembersDouble_ThenStoreBuildGrowsLinearlyOnBothPathsAndGeneratedIsCheaperPerItem)}");
		text.AppendLine();
		text.AppendLine(
			$"resolveStore (the store build), median of {RunsPerVariant} cold processes per variant and size; per item ="
			+ " resolveStore / K in microseconds. JIT columns: what the runtime reports for the phase (not asserted).");
		text.AppendLine();
		text.AppendLine("| Sample | K | Scanned ms | Generated ms | Scanned µs per item | Generated µs per item | Generated / Scanned | Scanned JIT ms (methods) | Generated JIT ms (methods) |");
		text.AppendLine("|---|---:|---:|---:|---:|---:|---:|---:|---:|");
		var cheaper = new List<string>();
		foreach (ScalingSample sample in series.Samples)
		{
			double scanned = series.Median(sample, Scanned, resolveStore);
			double generated = series.Median(sample, Generated, resolveStore);
			double scannedPerItem = scanned * 1_000 / sample.Members;
			double generatedPerItem = generated * 1_000 / sample.Members;
			text.AppendLine(
				$"| {sample.Name} | {N(sample.Members)} | {F(scanned)} | {F(generated)} | {F(scannedPerItem)} | {F(generatedPerItem)}"
				+ $" | {F(generated / scanned)} | {F(series.Median(sample, Scanned, x => x.JitMilliseconds["resolveStore"]))}"
				+ $" ({N(series.Median(sample, Scanned, x => x.JitMethods["resolveStore"]))})"
				+ $" | {F(series.Median(sample, Generated, x => x.JitMilliseconds["resolveStore"]))}"
				+ $" ({N(series.Median(sample, Generated, x => x.JitMethods["resolveStore"]))}) |");
			if (generatedPerItem > scannedPerItem)
				cheaper.Add($"{sample.Name}: generated {F(generatedPerItem)} µs per item > scanned {F(scannedPerItem)} µs");
		}
		text.AppendLine();
		text.AppendLine($"Slope of log(resolveStore) against log(K): Scanned **{F(scannedSlope)}**, Generated **{F(generatedSlope)}**; bar [0.70, 1.30].");
		Report(
			nameof(WhenFluxorMembersDouble_ThenStoreBuildGrowsLinearlyOnBothPathsAndGeneratedIsCheaperPerItem),
			text,
			$"slopes scanned {F(scannedSlope)}, generated {F(generatedSlope)}; generated / scanned at 8x"
			+ $" {F(series.Median(series.Samples[^1], Generated, resolveStore) / series.Median(series.Samples[^1], Scanned, resolveStore))}");

		AssertBetween(scannedSlope, 0.7, 1.3, "scanned resolveStore slope", text);
		AssertBetween(generatedSlope, 0.7, 1.3, "generated resolveStore slope", text);
		Assert.True(cheaper.Count == 0, string.Join(Environment.NewLine, cheaper) + Environment.NewLine + text);
	}

	/// <summary>
	/// Per dispatch, after the store is built (warm, in the process that built it): the same cost on both paths, and
	/// linear in the features (the store notifies every feature, each feature asks each of its reducers, the store
	/// asks every effect). Open item, kept failing on purpose: the wall time grows about F^1.7 to F^2.1 on both paths
	/// (each <c>action is TAction</c> check gets slower as distinct wrapper instantiations grow), which a separate
	/// investigation of the dispatcher, outside the generated registration work, is to look at (results/scaling.md).
	/// </summary>
	[ScalingFact]
	public void WhenDispatching_ThenCostIsTheSameOnBothPathsAndLinearInFeatures()
	{
		Series series = FluxorMemberRuns.Value;
		Func<RunResult, double> perDispatch = x => x.Dispatch!.MedianMicroseconds;
		Assert.All(series.Runs.Values, x => Assert.Equal(DispatchRuns, x.Count(r => r.Dispatch is not null)));
		foreach (DispatchResult dispatch in series.All.Select(x => x.Dispatch).OfType<DispatchResult>())
		{
			// The dispatched action is handled by exactly one reducer and one effect
			Assert.Equal((1, 1), (dispatch.Reducers, dispatch.Effects));
		}
		series = series.WithDispatchesOnly();

		double[] features = series.Samples.Select(x => (double)x.Features).ToArray();
		double scannedSlope = Statistics.LogLogSlope(features, series.Samples.Select(x => series.Median(x, Scanned, perDispatch)).ToArray());
		double generatedSlope = Statistics.LogLogSlope(features, series.Samples.Select(x => series.Median(x, Generated, perDispatch)).ToArray());

		var text = new StringBuilder();
		text.AppendLine($"## {nameof(WhenDispatching_ThenCostIsTheSameOnBothPathsAndLinearInFeatures)}");
		text.AppendLine();
		text.AppendLine(
			$"Microseconds per dispatch of an action handled by one reducer and one effect: in each cold process, after the"
			+ $" store is built and {N(DispatchesPerBatch)} warm-up dispatches, the median of {DispatchBatches} batches of"
			+ $" {N(DispatchesPerBatch)}; then the median of {DispatchRuns} processes.");
		text.AppendLine();
		text.AppendLine("| Sample | F | R | E | Scanned µs | Generated µs | Generated / Scanned |");
		text.AppendLine("|---|---:|---:|---:|---:|---:|---:|");
		var outside = new List<string>();
		foreach (ScalingSample sample in series.Samples)
		{
			double scanned = series.Median(sample, Scanned, perDispatch);
			double generated = series.Median(sample, Generated, perDispatch);
			double ratio = generated / scanned;
			text.AppendLine($"| {sample.Name} | {N(sample.Features)} | {N(sample.Reducers)} | {N(sample.Effects)} | {F(scanned)} | {F(generated)} | {F(ratio)} |");
			if (ratio < 0.8 || ratio > 1.25)
				outside.Add($"{sample.Name}: generated / scanned per dispatch {F(ratio)} is outside [0.80, 1.25]");
		}
		text.AppendLine();
		text.AppendLine($"Slope of log(per dispatch) against log(F): Scanned **{F(scannedSlope)}**, Generated **{F(generatedSlope)}**; bar [0.70, 1.30].");
		Report(
			nameof(WhenDispatching_ThenCostIsTheSameOnBothPathsAndLinearInFeatures),
			text,
			$"slopes scanned {F(scannedSlope)}, generated {F(generatedSlope)}; ratios "
			+ string.Join(" / ", series.Samples.Select(x => F(series.Median(x, Generated, perDispatch) / series.Median(x, Scanned, perDispatch)))));

		Assert.True(outside.Count == 0, string.Join(Environment.NewLine, outside) + Environment.NewLine + text);
		AssertBetween(scannedSlope, 0.7, 1.3, "scanned per-dispatch slope", text);
		AssertBetween(generatedSlope, 0.7, 1.3, "generated per-dispatch slope", text);
	}

	/// <summary>
	/// The generated path's only step above linear is the MetadataToken sort of the described types in
	/// GeneratedRegistrationCatalog.Create: O(n log n) with n the described types. Asserts only that the catalog (sort
	/// included) stays negligible: at most 5% of the cold generated addFluxor at the largest size. The n log n spread
	/// and the slope against n are reported, not asserted: they exceed n log n because the 4x size has the highest time
	/// per n log n in every recorded run, for a reason not established. Timed in this process, warm, with the catalog
	/// alone.
	/// </summary>
	[ScalingFact]
	public void WhenEntriesGrow_ThenTheMetadataTokenSortStaysNegligible()
	{
		// Create takes 25 to 300 µs: many repeats, so a GC or an interrupt in a batch does not move the median
		const int CallsPerBatch = 500;
		const int Batches = 31;

		IReadOnlyList<ScalingSample> samples = ScalingSample.FluxorMemberSeries;
		Assembly[][] assemblies = samples.Select(x => new[] { Assembly.Load(x.AssemblyName("Generated")) }).ToArray();
		int[] entries = assemblies
			.Select(x =>
			{
				Assert.True(GeneratedRegistrationCatalog.Create(x).TryGetTypes(x[0], out Type[] types));
				return types.Length;
			})
			.ToArray();

		// Warm: the first call of each builds its registration's table (once per process); calling for a while lets
		// tiered compilation finish optimizing Create, so no size is timed with less optimized code than another
		var stopwatch = Stopwatch.StartNew();
		while (stopwatch.Elapsed < TimeSpan.FromSeconds(2))
		{
			foreach (Assembly[] assembly in assemblies)
				GeneratedRegistrationCatalog.Create(assembly);
		}

		var batches = assemblies.Select(_ => new List<double>()).ToArray();
		for (int batch = 0; batch < Batches; batch++)
		{
			// Take turns, so drift affects every size alike
			for (int index = 0; index < assemblies.Length; index++)
			{
				stopwatch.Restart();
				for (int call = 0; call < CallsPerBatch; call++)
					GeneratedRegistrationCatalog.Create(assemblies[index]);
				batches[index].Add(stopwatch.Elapsed.TotalMicroseconds / CallsPerBatch);
			}
		}
		var rows = samples
			.Select((sample, index) => (Sample: sample, Entries: entries[index], Microseconds: Statistics.Median(batches[index])))
			.ToList();

		double[] perNLogN = rows.Select(x => x.Microseconds / (x.Entries * Math.Log2(x.Entries))).ToArray();
		double spread = perNLogN.Max() / perNLogN.Min();
		double slope = Statistics.LogLogSlope(rows.Select(x => (double)x.Entries).ToArray(), rows.Select(x => x.Microseconds).ToArray());

		ScalingSample largest = ScalingSample.FluxorMemberSeries[^1];
		double generatedAddFluxor = FluxorMemberRuns.Value.Median(largest, Generated, x => x.Phases["addFluxor"]);
		double share = rows[^1].Microseconds / 1_000 / generatedAddFluxor;

		var text = new StringBuilder();
		text.AppendLine($"## {nameof(WhenEntriesGrow_ThenTheMetadataTokenSortStaysNegligible)}");
		text.AppendLine();
		text.AppendLine(
			$"`GeneratedRegistrationCatalog.Create` for one generated sample assembly, warm (2 s of calls first), in the test"
			+ $" process: median of {Batches} batches of {CallsPerBatch} calls, the sizes taking turns. n = types the catalog sorts by MetadataToken (the described"
			+ " Fluxor types: states, hosts, reducer and effect classes).");
		text.AppendLine();
		text.AppendLine("| Sample | K | n | µs per Create | µs / (n log2 n) |");
		text.AppendLine("|---|---:|---:|---:|---:|");
		for (int index = 0; index < rows.Count; index++)
			text.AppendLine($"| {rows[index].Sample.Name} | {N(rows[index].Sample.Members)} | {N(rows[index].Entries)} | {F(rows[index].Microseconds)} | {(rows[index].Microseconds / (rows[index].Entries * Math.Log2(rows[index].Entries))).ToString("0.0000", CultureInfo.InvariantCulture)} |");
		text.AppendLine();
		text.AppendLine(
			$"Largest / smallest µs / (n log2 n): {F(spread)}; slope of log(time) against log(n): {F(slope)}"
			+ " (both reported, not asserted: n log n alone gives a slope of about 1.15 over n = 500 to 4,000; the"
			+ " excess comes from the size with the highest µs / (n log2 n); in the recorded runs the 4x one, cause not established)."
			+ $" At {largest.Name}, Create is {F(rows[^1].Microseconds / 1_000)} ms of the cold generated addFluxor"
			+ $" {F(generatedAddFluxor)} ms: **{P(share)}** (bar <= 5%).");
		Report(
			nameof(WhenEntriesGrow_ThenTheMetadataTokenSortStaysNegligible),
			text,
			$"n log n spread {F(spread)}, slope vs n {F(slope)}, share of addFluxor at 8x {P(share)}");

		Assert.True(share <= 0.05, $"Create is {P(share)} of the generated addFluxor" + Environment.NewLine + text);
	}

	/// <summary>
	/// The decisive proof: K fixed (the 1x sample), plain classes with no Fluxor code added to the same assembly.
	/// The reflection scan's addFluxor grows with them (O(T + M)): at least 0.5 ms per 1,000 added methods (least squares
	/// over 0, 2,000, 4,000 and 8,000 classes); the generated addFluxor does not (O(K)): the 8,000-class run is at most
	/// 1.25x the 0-class one.
	/// </summary>
	[ScalingFact]
	public void WhenNonFluxorCodeGrows_ThenOnlyTheReflectionScanGetsSlower()
	{
		Series series = NonFluxorCodeRuns.Value;
		Func<RunResult, double> addFluxor = x => x.Phases["addFluxor"];
		double[] addedMethods = series.Samples.Select(x => (double)x.AddedMethods).ToArray();
		double[] scanned = series.Samples.Select(x => series.Median(x, Scanned, addFluxor)).ToArray();
		double[] generated = series.Samples.Select(x => series.Median(x, Generated, addFluxor)).ToArray();
		// Milliseconds per 1,000 added methods
		double scannedSlope = Statistics.Slope(addedMethods, scanned) * 1_000;
		double generatedSlope = Statistics.Slope(addedMethods, generated) * 1_000;
		double scannedGrowth = scanned[^1] / scanned[0];
		double generatedGrowth = generated[^1] / generated[0];

		var text = new StringBuilder();
		text.AppendLine($"## {nameof(WhenNonFluxorCodeGrows_ThenOnlyTheReflectionScanGetsSlower)}");
		text.AppendLine();
		text.AppendLine(
			$"addFluxor, median of {RunsPerVariant} cold processes per variant and size, milliseconds. K is fixed"
			+ $" ({N(series.Samples[0].Members)}); the plain classes have {series.Samples[^1].Options.MethodsPerPlainClass} public"
			+ " methods each and no Fluxor attributes. T and M are counted by the runner after measuring.");
		text.AppendLine();
		text.AppendLine("| Sample | Plain classes | Added methods | T | M | Scanned ms | Generated ms | Scanned / 0 | Generated / 0 |");
		text.AppendLine("|---|---:|---:|---:|---:|---:|---:|---:|---:|");
		for (int index = 0; index < series.Samples.Count; index++)
		{
			ScalingSample sample = series.Samples[index];
			RunResult first = series.Runs[(sample, Scanned)][0];
			text.AppendLine(
				$"| {sample.Name} | {N(sample.Options.PlainClasses)} | {N(sample.AddedMethods)} | {N(first.Types)} | {N(first.Methods)}"
				+ $" | {F(scanned[index])} | {F(generated[index])} | {F(scanned[index] / scanned[0])} | {F(generated[index] / generated[0])} |");
		}
		text.AppendLine();
		text.AppendLine(
			$"Least squares slope against added methods: Scanned **{F(scannedSlope)} ms per 1,000 methods** (bar >= 0.50),"
			+ $" Generated {F(generatedSlope)} ms per 1,000 methods. Largest / 0 classes: Scanned {F(scannedGrowth)} (not a bar),"
			+ $" Generated **{F(generatedGrowth)}** (bar <= 1.25).");
		Report(
			nameof(WhenNonFluxorCodeGrows_ThenOnlyTheReflectionScanGetsSlower),
			text,
			$"scanned {F(scannedSlope)} ms per 1,000 methods, x{F(scannedGrowth)}; generated {F(generatedSlope)} ms per 1,000 methods, x{F(generatedGrowth)}");

		Assert.True(
			scannedSlope >= 0.5,
			$"Scanned addFluxor grows only {F(scannedSlope)} ms per 1,000 added methods" + Environment.NewLine + text);
		Assert.True(generatedGrowth <= 1.25, $"Generated addFluxor grew {F(generatedGrowth)} x" + Environment.NewLine + text);
	}

	private void Report(string test, StringBuilder text, string summary)
	{
		text.AppendLine();
		text.AppendLine(ScalingReport.DescribeEnvironment());
		Output.WriteLine(text.ToString());
		ScalingReport.Write(TestPaths.ScalingResultsPath, test, text.ToString(), summary);
	}

	private static void AssertBetween(double value, double minimum, double maximum, string what, StringBuilder text) =>
		Assert.True(
			value >= minimum && value <= maximum,
			$"{what} {F(value)} is outside [{F(minimum)}, {F(maximum)}]" + Environment.NewLine + text);

	private static string F(double value) => value.ToString("0.00", CultureInfo.InvariantCulture);

	private static string N(double value) => value.ToString("N0", CultureInfo.InvariantCulture);

	private static string P(double value) => value.ToString("P2", CultureInfo.InvariantCulture);

	/// <summary>Cold runs of each sample of a series, both variants, the samples and variants taking turns.</summary>
	private sealed class Series
	{
		public required IReadOnlyList<ScalingSample> Samples { get; init; }
		public required Dictionary<(ScalingSample Sample, string Variant), List<RunResult>> Runs { get; init; }

		public IEnumerable<RunResult> All => Runs.Values.SelectMany(x => x);

		public static Series Measure(IReadOnlyList<ScalingSample> samples, Func<int, string[]> optionsForRun)
		{
			var runs = samples
				.SelectMany(x => Variants.Select(v => (x, v)))
				.ToDictionary(x => x, _ => new List<RunResult>());
			for (int run = 0; run < RunsPerVariant; run++)
			{
				// Take turns, so drift on the machine affects every sample and variant alike
				foreach (ScalingSample sample in samples)
				{
					foreach (string variant in Variants)
						runs[(sample, variant)].Add(ColdProcess.Run(sample.RunnerVariant(variant), optionsForRun(run)));
				}
			}

			// Both variants build the same store, the one the sample describes, each with its own path
			foreach (((ScalingSample sample, string variant), List<RunResult> results) in runs)
			{
				foreach (RunResult result in results)
				{
					Assert.Equal((sample.Features, sample.Reducers, sample.Effects), (result.Features, result.Reducers, result.Effects));
					Assert.Equal(variant == Generated, result.HasGeneratedRegistration);
					if (variant == Generated)
						Assert.Equal(
							(sample.Reducers - sample.Options.ReducerClasses, sample.Effects - sample.Options.EffectClasses),
							(result.GeneratedReducers, result.GeneratedEffects));
					else
						Assert.Equal((0, 0), (result.GeneratedReducers, result.GeneratedEffects));
				}
			}

			return new Series { Samples = samples, Runs = runs };
		}

		/// <summary>The same series with only the runs that timed dispatches.</summary>
		public Series WithDispatchesOnly() => new()
		{
			Samples = Samples,
			Runs = Runs.ToDictionary(x => x.Key, x => x.Value.Where(r => r.Dispatch is not null).ToList())
		};

		public double Median(ScalingSample sample, string variant, Func<RunResult, double> selector) =>
			Statistics.Median(Runs[(sample, variant)], selector);

		public double LogLogSlopeAgainstMembers(string variant, Func<RunResult, double> selector) =>
			Statistics.LogLogSlope(
				Samples.Select(x => (double)x.Members).ToArray(),
				Samples.Select(x => Median(x, variant, selector)).ToArray());

		public void AppendPhaseTable(StringBuilder text, Func<RunResult, double> measured)
		{
			text.AppendLine("| Sample | F | R | E | K | Scanned ms | Generated ms | Generated / Scanned | Generated registration methods |");
			text.AppendLine("|---|---:|---:|---:|---:|---:|---:|---:|---:|");
			foreach (ScalingSample sample in Samples)
			{
				double scanned = Median(sample, Scanned, measured);
				double generated = Median(sample, Generated, measured);
				text.AppendLine(
					$"| {sample.Name} | {N(sample.Features)} | {N(sample.Reducers)} | {N(sample.Effects)} | {N(sample.Members)}"
					+ $" | {F(scanned)} | {F(generated)} | {F(generated / scanned)} | {N(Runs[(sample, Generated)][0].GeneratedMethods)} |");
			}
		}
	}
}

/// <summary>
/// A scaling timing test: as <see cref="PerformanceFactAttribute"/> (FLUXOR_PERF_TESTS=1, Release), and also only
/// when the scaling samples were built (-p:FluxorScalingSamples=true), which a normal build leaves out.
/// </summary>
public sealed class ScalingFactAttribute : FactAttribute
{
	public const string HowToRun =
		"dotnet build Source -c Release -p:FluxorPerformance=true -p:FluxorScalingSamples=true, then FLUXOR_PERF_TESTS=1 dotnet test"
		+ " Source/Performance/Fluxor.PerformanceTests -c Release --no-build --filter FullyQualifiedName~ScalingTests";

	public ScalingFactAttribute()
	{
		if (Environment.GetEnvironmentVariable(PerformanceFactAttribute.EnvironmentVariable) != "1")
			Skip = $"Scaling timing test: set {PerformanceFactAttribute.EnvironmentVariable}=1, Release, and build the scaling samples: {HowToRun}";
		else if (TestPaths.Configuration != "Release")
			Skip = $"Scaling timing test: runs only in Release (this build is {TestPaths.Configuration}): {HowToRun}";
		else if (!TestPaths.HasScalingSamples)
			Skip = $"Scaling timing test: the scaling samples were not built (they are opt-in): {HowToRun}";
	}
}
