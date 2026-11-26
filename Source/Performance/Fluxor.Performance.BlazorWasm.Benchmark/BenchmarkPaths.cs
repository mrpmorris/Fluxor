using System.Reflection;

namespace Fluxor.Performance.BlazorWasm.Benchmark;

internal static class BenchmarkPaths
{
	public static string AppProjectPath => GetMetadata("AppProjectPath");
	public static string ResultsFilePath => GetMetadata("ResultsFilePath");

	/// <summary>Publish output, kept between runs so later publishes are incremental.</summary>
	public static string WorkDirectory => Path.Combine(Path.GetTempPath(), "fluxor-wasm-benchmark");

	private static string GetMetadata(string key) =>
		typeof(BenchmarkPaths).Assembly
			.GetCustomAttributes<AssemblyMetadataAttribute>()
			.Single(x => x.Key == key)
			.Value!;
}
