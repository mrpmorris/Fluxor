namespace Fluxor.Performance.BlazorWasm.Benchmark;

/// <summary>
/// A browser benchmark: it runs only when FLUXOR_WASM_BENCHMARK=1, and is skipped with the reason otherwise. It
/// publishes six app variants (two more for the non-Fluxor code comparison) and starts each in headless Chromium
/// many times, which takes minutes, so it is not part of the normal test run.
/// </summary>
public sealed class WasmBenchmarkTheoryAttribute : TheoryAttribute
{
	public const string EnvironmentVariable = "FLUXOR_WASM_BENCHMARK";

	public const string HowToRun =
		"dotnet build Source -c Release -p:FluxorPerformance=true, then pwsh"
		+ " Source/Performance/Fluxor.Performance.BlazorWasm.Benchmark/bin/Release/net10.0/playwright.ps1 install chromium,"
		+ " then FLUXOR_WASM_BENCHMARK=1 dotnet test Source/Performance/Fluxor.Performance.BlazorWasm.Benchmark -c Release --no-build";

	public WasmBenchmarkTheoryAttribute()
	{
		if (Environment.GetEnvironmentVariable(EnvironmentVariable) != "1")
			Skip = $"Browser benchmark: set {EnvironmentVariable}=1 to run it (it publishes the app six times, and twice more for the non-Fluxor code comparison): {HowToRun}";
	}
}
