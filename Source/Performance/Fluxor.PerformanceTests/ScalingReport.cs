using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Fluxor.PerformanceTests;

/// <summary>
/// Writes results/scaling.md: the hand-written complexity analysis (kept as it is), one section per scaling test
/// (replaced by each run of that test) and a log of every run's key numbers (appended to).
/// </summary>
internal static class ScalingReport
{
	private const string AnalysisKey = "analysis";
	private const string RunsKey = "runs";

	private static readonly string[] TestKeys =
	[
		nameof(ScalingTests.WhenFluxorMembersDouble_ThenBothPathsGrowLinearly),
		nameof(ScalingTests.WhenFluxorMembersDouble_ThenStoreBuildGrowsLinearlyOnBothPathsAndGeneratedIsCheaperPerItem),
		nameof(ScalingTests.WhenDispatching_ThenCostIsTheSameOnBothPathsAndLinearInFeatures),
		nameof(ScalingTests.WhenEntriesGrow_ThenTheMetadataTokenSortStaysNegligible),
		nameof(ScalingTests.WhenNonFluxorCodeGrows_ThenOnlyTheReflectionScanGetsSlower)
	];

	private const string Header = """
		# Scaling of Fluxor registration: generated against the reflection scan

		The analysis is written by hand; the measured sections after it are written by `ScalingTests` in
		`Source/Performance/Fluxor.PerformanceTests` (on demand: `FLUXOR_PERF_TESTS=1`, Release), each replaced when its
		test runs, and the run log at the end gets one line per test run. The samples are written at build time by
		`Fluxor.Performance.SampleWriter` (`ScalingSample`) into the obj folder of each sample project under
		`Source/Performance/Samples/Scaling`, and measured in cold processes of `Fluxor.Performance.Runner` (CoreCLR).

		The performance projects and the scaling samples are opt-in, so a normal `dotnet build Source` does not build them
		(the scaling samples are tens of MB of generated code). To run the tests:

		    dotnet build Source -c Release -p:FluxorPerformance=true -p:FluxorScalingSamples=true
		    FLUXOR_PERF_TESTS=1 dotnet test Source/Performance/Fluxor.PerformanceTests -c Release --no-build --filter FullyQualifiedName~ScalingTests

		Without the scaling samples the tests skip and say so. A run takes about 10 minutes.

		""";

	/// <summary>Replaces the test's section and adds a line to the run log.</summary>
	public static void Write(string path, string test, string section, string summary)
	{
		string existing = File.Exists(path) ? Normalize(File.ReadAllText(path)) : "";

		var text = new StringBuilder(Normalize(Header));
		foreach (string key in TestKeys.Prepend(AnalysisKey).Append(RunsKey))
		{
			string body = key == test
				? Normalize(section).Trim('\n')
				: FindSection(existing, key);
			if (key == RunsKey)
			{
				if (body.Length == 0)
					body = "## Run log\n\n| Date (UTC) | Test | Result | SDK |\n|---|---|---|---|";
				body += "\n" + string.Create(
					CultureInfo.InvariantCulture,
					$"| {DateTime.UtcNow:yyyy-MM-dd HH:mm} | {test} | {summary} | {TestPaths.SdkVersion} |");
			}
			if (body.Length == 0)
				continue;
			text.Append('\n');
			text.Append(Begin(key)).Append('\n');
			text.Append(body).Append('\n');
			text.Append(End(key)).Append('\n');
		}

		Directory.CreateDirectory(Path.GetDirectoryName(path)!);
		File.WriteAllText(path, text.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
	}

	/// <summary>Where and with what the test ran.</summary>
	public static string DescribeEnvironment() =>
		string.Create(
			CultureInfo.InvariantCulture,
			$"Measured {DateTime.UtcNow:yyyy-MM-dd} (UTC) on {GetProcessorName()} ({Environment.ProcessorCount} logical processors),"
			+ $" {RuntimeInformation.OSDescription}; {RuntimeInformation.FrameworkDescription} (CoreCLR, the runner's runtime);"
			+ $" built with .NET SDK {TestPaths.SdkVersion}, {TestPaths.Configuration}.");

	private static string FindSection(string text, string key)
	{
		int start = text.IndexOf(Begin(key), StringComparison.Ordinal);
		int end = text.IndexOf(End(key), StringComparison.Ordinal);
		if (start < 0 || end < start)
			return "";
		start += Begin(key).Length;
		return text[start..end].Trim('\n');
	}

	private static string Begin(string key) => $"<!-- {key}: begin -->";

	private static string End(string key) => $"<!-- {key}: end -->";

	private static string Normalize(string text) => text.Replace("\r\n", "\n");

	private static string GetProcessorName()
	{
		if (OperatingSystem.IsWindows())
		{
			using Microsoft.Win32.RegistryKey? key =
				Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
			if (key?.GetValue("ProcessorNameString") is string name)
				return name.Trim();
		}
		return RuntimeInformation.ProcessArchitecture.ToString();
	}
}
