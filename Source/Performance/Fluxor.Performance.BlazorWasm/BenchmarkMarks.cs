using System.Runtime.InteropServices.JavaScript;

namespace Fluxor.Performance.BlazorWasm;

/// <summary>
/// Sets the <c>performance.mark</c>s the benchmark reads, and reports the store's shape, through
/// <c>window.fluxorBench</c> (set up in index.html before Blazor starts).
/// </summary>
internal static partial class BenchmarkMarks
{
	public const string Main = "fluxor:main";
	public const string BeforeAddFluxor = "fluxor:before-add-fluxor";
	public const string AfterAddFluxor = "fluxor:after-add-fluxor";
	public const string AfterBuild = "fluxor:after-build";
	public const string StoreInitialized = "fluxor:store-initialized";
	public const string FirstRender = "fluxor:first-render";

	[JSImport("globalThis.fluxorBench.mark")]
	public static partial void Mark(string name);

	/// <summary>Reports the store's shape as JSON, after every mark is set; the benchmark then reads the results.</summary>
	[JSImport("globalThis.fluxorBench.report")]
	public static partial void Report(string json);
}
