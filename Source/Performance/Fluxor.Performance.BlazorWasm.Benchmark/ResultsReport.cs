using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Fluxor.Performance.BlazorWasm.Benchmark;

/// <summary>
/// Writes the results as Markdown: one section per sample size. The results file keeps the sections of sizes not
/// measured in this run, so running one size does not lose the others, and keeps the hand-written historical
/// comparison at its end.
/// </summary>
internal static class ResultsReport
{
	private static readonly string[] Sizes = ["small", "medium", "complex"];

	/// <summary>The section of the non-Fluxor code comparison (reported, no bar), after the sizes' sections.</summary>
	public const string NonFluxorCodeKey = "complex-non-fluxor-code";

	/// <summary>The section this benchmark does not measure: kept as it is in the file.</summary>
	private const string HistoryKey = "history";

	private const string Header = """
		# Blazor WebAssembly start-up: generated registration against the reflection scan

		Written by `WasmBenchmarkTests.WhenAppStartsInTheBrowser_ThenGeneratedRegistrationIsFaster` in
		`Source/Performance/Fluxor.Performance.BlazorWasm.Benchmark` (on demand: `FLUXOR_WASM_BENCHMARK=1`). Do not edit,
		except the historical comparison at the end, which the benchmark keeps as it is.

		To run it (it builds only with the opt-in performance projects):

		    dotnet build Source -c Release -p:FluxorPerformance=true
		    pwsh Source/Performance/Fluxor.Performance.BlazorWasm.Benchmark/bin/Release/net10.0/playwright.ps1 install chromium
		    FLUXOR_WASM_BENCHMARK=1 dotnet test Source/Performance/Fluxor.Performance.BlazorWasm.Benchmark -c Release --no-build

		The app (`Source/Performance/Fluxor.Performance.BlazorWasm`) is published with `dotnet publish -c Release`
		(Mono interpreter, no AOT) once per variant, selected with `-p:FluxorSampleSize=Small|Medium|Complex` and
		`-p:FluxorSample=Scanned|Generated`; `Generated` is the generated registration, labelled by the shape of its
		code, Compact. Each is served as static files from Kestrel and loaded in headless
		Chromium. Phases come from `performance.mark` in the app: Boot = navigation start to `Main`; Host builder =
		`WebAssemblyHostBuilder.CreateDefault`; AddFluxor = the `AddFluxor` call; Build = `builder.Build()`; Store
		initialized = after `Build()` to `IStore.Initialized` completed (host start, first render, creating and
		initializing the store); First render = navigation start to the page's first render. Total blocked = the
		part over 50 ms of every long task (`PerformanceObserver` `longtask`) that started before the later of
		first render and store initialized; Longest task = the longest of those tasks.

		""";

	public static string CreateSection(
		SampleSize size,
		IReadOnlyList<Variant> variants,
		IReadOnlyDictionary<Variant, List<LoadResult>> loads,
		int warmUpLoads,
		string environment)
	{
		Variant scanned = variants.Single(x => !x.IsGenerated);
		double scannedMeasured = Median(loads[scanned], x => x.Measured);
		Variant compact = variants.Single(x => x.Build == Variant.Generated);
		double compactRatio = Median(loads[compact], x => x.Measured) / scannedMeasured;

		var text = new StringBuilder();
		text.AppendLine(Invariant($"## {size.Key}: {size.Features} features, {size.Reducers} reducers, {size.Effects} effects"));
		text.AppendLine();
		text.AppendLine((
			$"Median of {loads[scanned].Count} loads per variant (after {warmUpLoads} discarded warm-up load per variant), the variants"
			+ " taking turns, each load in a new browser context. Milliseconds; ratio = AddFluxor + store initialized against Scanned."));
		text.AppendLine();
		text.AppendLine("| Variant | Boot (to Main) | Host builder | AddFluxor | Build | Store initialized | **AddFluxor + store initialized** | **Ratio** | First render | Total blocked | Longest task |");
		text.AppendLine("|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|");
		foreach (Variant variant in variants)
		{
			List<LoadResult> results = loads[variant];
			double measured = Median(results, x => x.Measured);
			text.AppendLine(
				$"| {variant.Label} | {F(Median(results, x => x.Boot))} | {F(Median(results, x => x.HostBuilder))}"
				+ $" | {F(Median(results, x => x.AddFluxor))} | {F(Median(results, x => x.Build))}"
				+ $" | {F(Median(results, x => x.StoreInitialized))} | **{F(measured)}** | **{F(measured / scannedMeasured)}**"
				+ $" | {F(Median(results, x => x.FirstRender))} | {F(Median(results, x => x.TotalBlocked))}"
				+ $" | {F(Median(results, x => x.LongestTask))} |");
		}

		text.AppendLine();
		text.AppendLine(
			"Store shape (the same in every load of a variant): method reducers/effects are those made from"
			+ " `[ReducerMethod]`/`[EffectMethod]`; in brackets, how many of them generated code created.");
		text.AppendLine();
		text.AppendLine("| Variant | Features | Reducers | Effects | Method reducers (generated) | Method effects (generated) | Generated registration |");
		text.AppendLine("|---|---:|---:|---:|---:|---:|---|");
		foreach (Variant variant in variants)
		{
			StoreShape shape = loads[variant][0].Shape;
			text.AppendLine((
				$"| {variant.Label} | {shape.Features} | {shape.Reducers} | {shape.Effects}"
				+ $" | {shape.MethodReducers} ({shape.GeneratedReducers}) | {shape.MethodEffects} ({shape.GeneratedEffects})"
				+ $" | {(shape.HasGeneratedRegistration ? "yes" : "no")} |"));
		}

		text.AppendLine();
		text.AppendLine(
			$"Bar (Compact): ratio {size.DescribeBar()}; measured {F(compactRatio)}: {(size.Passes(compactRatio) ? "pass" : "FAIL")}.");
		text.AppendLine();
		text.AppendLine(environment);
		return text.ToString();
	}

	/// <summary>
	/// The complex sample with 0 and with 4,000 plain classes (10 public methods each, no Fluxor code), Scanned and
	/// Generated: how code that is not Fluxor's changes the start-up. Reported only, no bar.
	/// </summary>
	public static string CreateNonFluxorCodeSection(
		IReadOnlyList<Variant> variants,
		IReadOnlyDictionary<Variant, List<LoadResult>> loads,
		int warmUpLoads,
		string environment)
	{
		var text = new StringBuilder();
		text.AppendLine("## complex with 0 and 4,000 plain classes: code that is not Fluxor's");
		text.AppendLine();
		text.AppendLine((
			"Written by `WasmBenchmarkTests.WhenComplexSampleGetsNonFluxorCode_ThenTheStartUpIsReported`. Reported only, no bar."
			+ " ComplexPlain4000 is the complex sample (200 features, 2,000 reducers, 2,000 effects) plus 4,000 plain classes of"
			+ " 10 public methods each in the same assembly, with no Fluxor code (`ScalingSample.ComplexPlain4000`, written at"
			+ $" build time). Median of {loads[variants[0]].Count} loads per variant (after {warmUpLoads} discarded warm-up load"
			+ " per variant), the four variants taking turns, each load in a new browser context. Milliseconds; vs 0 classes ="
			+ " against the same build of the complex sample without plain classes."));
		text.AppendLine();
		text.AppendLine("| Sample | Variant | Boot (to Main) | AddFluxor | AddFluxor vs 0 classes | Store initialized | **AddFluxor + store initialized** | **vs 0 classes** | First render |");
		text.AppendLine("|---|---|---:|---:|---:|---:|---:|---:|---:|");
		foreach (Variant variant in variants)
		{
			List<LoadResult> results = loads[variant];
			Variant without = variants.First(x => x.Build == variant.Build);
			double addFluxor = Median(results, x => x.AddFluxor);
			double measured = Median(results, x => x.Measured);
			text.AppendLine(
				$"| {variant.Size} | {variant.Label} | {F(Median(results, x => x.Boot))} | {F(addFluxor)}"
				+ $" | {F(addFluxor / Median(loads[without], x => x.AddFluxor))} | {F(Median(results, x => x.StoreInitialized))}"
				+ $" | **{F(measured)}** | **{F(measured / Median(loads[without], x => x.Measured))}** | {F(Median(results, x => x.FirstRender))} |");
		}

		text.AppendLine();
		text.AppendLine("| Sample | Variant | Features | Reducers | Effects | Method reducers (generated) | Method effects (generated) |");
		text.AppendLine("|---|---|---:|---:|---:|---:|---:|");
		foreach (Variant variant in variants)
		{
			StoreShape shape = loads[variant][0].Shape;
			text.AppendLine((
				$"| {variant.Size} | {variant.Label} | {shape.Features} | {shape.Reducers} | {shape.Effects}"
				+ $" | {shape.MethodReducers} ({shape.GeneratedReducers}) | {shape.MethodEffects} ({shape.GeneratedEffects}) |"));
		}

		text.AppendLine();
		text.AppendLine(environment);
		return text.ToString();
	}

	/// <summary>
	/// Writes the results file with this size's section replaced, and the other sizes' sections and the historical
	/// comparison kept.
	/// </summary>
	public static void Write(string path, SampleSize size, string section) => Write(path, size.Key, section);

	/// <summary>
	/// Writes the results file with the section of <paramref name="sectionKey"/> (a size's key or
	/// <see cref="NonFluxorCodeKey"/>) replaced, and the other sections and the historical comparison kept.
	/// </summary>
	public static void Write(string path, string sectionKey, string section)
	{
		string existing = File.Exists(path) ? Normalize(File.ReadAllText(path)) : "";

		var text = new StringBuilder(Normalize(Header));
		foreach (string key in Sizes.Append(NonFluxorCodeKey).Append(HistoryKey))
		{
			string body = key == sectionKey ? Normalize(section).Trim('\n') : FindSection(existing, key);
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

	/// <summary>Where and with what the benchmark ran.</summary>
	public static string DescribeEnvironment(string browserVersion, string appRuntime)
	{
		string playwright = typeof(Microsoft.Playwright.IPlaywright).Assembly.GetName().Version?.ToString(3) ?? "?";
		return (
			$"Measured {DateTime.UtcNow:yyyy-MM-dd} (UTC) on {GetProcessorName()} ({Environment.ProcessorCount} logical processors),"
			+ $" {RuntimeInformation.OSDescription}; Chromium {browserVersion} headless (Playwright {playwright});"
			+ $" app runtime {appRuntime}; .NET SDK {GetSdkVersion()}.");
	}

	public static double Median(IEnumerable<LoadResult> results, Func<LoadResult, double> selector)
	{
		double[] values = results.Select(selector).Order().ToArray();
		int middle = values.Length / 2;
		return values.Length % 2 == 1
			? values[middle]
			: (values[middle - 1] + values[middle]) / 2;
	}

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

	private static string F(double value) => value.ToString("0.00", CultureInfo.InvariantCulture);

	private static string Invariant(FormattableString text) => text.ToString(CultureInfo.InvariantCulture);

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

	private static string GetSdkVersion()
	{
		var startInfo = new ProcessStartInfo("dotnet", "--version")
		{
			RedirectStandardOutput = true,
			UseShellExecute = false,
			WorkingDirectory = Path.GetDirectoryName(BenchmarkPaths.AppProjectPath)!
		};
		using Process process = Process.Start(startInfo)!;
		string version = process.StandardOutput.ReadToEnd().Trim();
		process.WaitForExit();
		return version;
	}
}
