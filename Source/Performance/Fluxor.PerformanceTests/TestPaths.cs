using System.Reflection;

namespace Fluxor.PerformanceTests;

internal static class TestPaths
{
	public static string RunnerPath => GetMetadata("RunnerPath");
	public static string Configuration => GetMetadata("Configuration");
	public static string ScalingResultsPath => GetMetadata("ScalingResultsPath");
	public static string SdkVersion => GetMetadata("SdkVersion");
	public static bool HasScalingSamples => GetMetadata("ScalingSamples") == "true";

	private static string GetMetadata(string key) =>
		typeof(TestPaths).Assembly
			.GetCustomAttributes<AssemblyMetadataAttribute>()
			.Single(x => x.Key == key)
			.Value!;
}
