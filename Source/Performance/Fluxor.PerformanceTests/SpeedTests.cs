using System.Globalization;
using System.Text;
using Xunit.Abstractions;

namespace Fluxor.PerformanceTests;

public class SpeedTests
{
	private const int RunsPerVariant = 7;
	private const string Scanned = "scanned";
	private const string Generated = "generated";
	private static readonly string[] Variants = [Scanned, Generated];

	private readonly ITestOutputHelper Output;

	public SpeedTests(ITestOutputHelper output)
	{
		Output = output;
	}

	/// <summary>
	/// CoreCLR guard for Blazor Server and MAUI apps that opt in: the generated registration must not be slower
	/// than the reflection scan on addFluxor + resolveStore. (The browser, on Mono, has its own benchmark.)
	/// </summary>
	[PerformanceFact]
	public void WhenRunCold_ThenGeneratedRegistrationIsNotSlowerThanReflection()
	{
		var runs = Variants.ToDictionary(x => x, _ => new List<RunResult>());
		for (int run = 0; run < RunsPerVariant; run++)
		{
			// Alternate, so drift on the machine affects every variant alike
			foreach (string variant in Variants)
				runs[variant].Add(ColdProcess.Run(variant));
		}

		// Every variant must build the same store, so none can be faster by registering less
		foreach (RunResult result in runs.Values.SelectMany(x => x))
		{
			Assert.Equal(200, result.Features);
			Assert.Equal(2_000, result.Reducers);
			Assert.Equal(2_000, result.Effects);
		}
		Assert.All(runs[Scanned], x => Assert.False(x.HasGeneratedRegistration));
		Assert.All(runs[Generated], x => Assert.True(x.HasGeneratedRegistration));
		// The measured process really used its path: every method reducer and effect (all but the 10 reducer and
		// 10 effect classes) was created by generated code, or none was
		Assert.All(runs[Scanned], x => Assert.Equal((0, 0), (x.GeneratedReducers, x.GeneratedEffects)));
		Assert.All(runs[Generated], x => Assert.Equal((1_990, 1_990), (x.GeneratedReducers, x.GeneratedEffects)));

		var table = new StringBuilder();
		table.AppendLine($"Median of {RunsPerVariant} cold processes per variant; ratios are against Scanned");
		AppendTable(table, "Phase ms", runs, phase => x => x.Phases[phase], includeSum: true);
		// Not asserted: how much of each phase is JIT, and how many methods it compiled, to explain the numbers above
		AppendTable(table, "JIT ms in phase", runs, phase => x => x.JitMilliseconds[phase], includeSum: true);
		AppendTable(table, "JIT methods in phase", runs, phase => x => x.JitMethods[phase], includeSum: true);
		table.AppendLine();
		table.AppendLine($"Methods in the generated registration: {runs[Generated][0].GeneratedMethods}");
		Output.WriteLine(table.ToString());

		double scannedMeasured = Median(runs[Scanned], Measured);
		double generatedMeasured = Median(runs[Generated], Measured);
		Assert.True(
			generatedMeasured <= scannedMeasured,
			$"Generated {Format(generatedMeasured)} ms is slower than scanned {Format(scannedMeasured)} ms" + Environment.NewLine + table);
	}

	private static void AppendTable(
		StringBuilder table,
		string title,
		Dictionary<string, List<RunResult>> runs,
		Func<string, Func<RunResult, double>> selectorForPhase,
		bool includeSum)
	{
		table.AppendLine();
		table.AppendLine($"| {title} | Scanned | Generated | Generated ratio |");
		table.AppendLine("|---|---:|---:|---:|");
		foreach (string phase in ColdProcess.PhaseNames)
			AppendRow(table, phase, runs, selectorForPhase(phase));
		if (includeSum)
			AppendRow(
				table,
				"**addFluxor + resolveStore**",
				runs,
				x => selectorForPhase("addFluxor")(x) + selectorForPhase("resolveStore")(x));
	}

	private static void AppendRow(StringBuilder table, string phase, Dictionary<string, List<RunResult>> runs, Func<RunResult, double> selector)
	{
		double scanned = Median(runs[Scanned], selector);
		double generated = Median(runs[Generated], selector);
		table.AppendLine($"| {phase} | {Format(scanned)} | {Format(generated)} | {Format(generated / scanned)} |");
	}

	private static double Measured(RunResult result) =>
		result.Phases["addFluxor"] + result.Phases["resolveStore"];

	private static string Format(double value) => value.ToString("0.00", CultureInfo.InvariantCulture);

	private static double Median(IEnumerable<RunResult> results, Func<RunResult, double> selector) =>
		Statistics.Median(results, selector);
}

/// <summary>
/// A cold-process timing test: it runs only when FLUXOR_PERF_TESTS=1 and the tests are built in Release, and is
/// skipped with the reason otherwise. Wall-clock bars are not reliable on shared CI runners or in Debug, so it is
/// not part of the normal test run.
/// </summary>
public sealed class PerformanceFactAttribute : FactAttribute
{
	public const string EnvironmentVariable = "FLUXOR_PERF_TESTS";

	public const string HowToRun =
		"dotnet build Source -c Release -p:FluxorPerformance=true, then FLUXOR_PERF_TESTS=1 dotnet test"
		+ " Source/Performance/Fluxor.PerformanceTests -c Release --no-build --filter FullyQualifiedName~SpeedTests";

	public PerformanceFactAttribute()
	{
		if (Environment.GetEnvironmentVariable(EnvironmentVariable) != "1")
			Skip = $"Timing test: set {EnvironmentVariable}=1 and run in Release to run it: {HowToRun}";
		else if (TestPaths.Configuration != "Release")
			Skip = $"Timing test: runs only in Release (this build is {TestPaths.Configuration}): {HowToRun}";
	}
}
