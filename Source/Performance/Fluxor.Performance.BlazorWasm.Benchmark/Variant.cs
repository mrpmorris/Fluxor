namespace Fluxor.Performance.BlazorWasm.Benchmark;

/// <summary>
/// One build of the app: which sample size it references, and how that sample registers with Fluxor. The values
/// are the app's MSBuild properties: <c>-p:FluxorSampleSize=Small|Medium|Complex</c>,
/// <c>-p:FluxorSample=Scanned|Generated</c>.
/// </summary>
internal sealed record Variant(string Size, string Build)
{
	public const string Scanned = "Scanned";
	public const string Generated = "Generated";

	public static readonly string[] Builds = [Scanned, Generated];

	public string Name => $"{Size}-{Build}";

	/// <summary>The name used in tables.</summary>
	public string Label => Build switch
	{
		Generated => "Compact",
		_ => Build
	};

	public bool IsGenerated => Build != Scanned;
}
