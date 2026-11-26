using System.Globalization;
using Xunit.Abstractions;

namespace Fluxor.Performance.BlazorWasm.Benchmark;

public class WasmBenchmarkTests
{
	private const int LoadsPerVariant = 10;
	private const int WarmUpLoadsPerVariant = 1;

	private readonly ITestOutputHelper Output;

	public WasmBenchmarkTests(ITestOutputHelper output)
	{
		Output = output;
	}

	/// <summary>
	/// The speed gate of the generated registration: Blazor WebAssembly runs on the Mono interpreter, where the
	/// reflection scan costs most. Publishes the app twice for the size (Scanned and Generated), starts each in
	/// headless Chromium, and compares AddFluxor + store initialized. Both builds must create the same store, and
	/// the generated build must have created every method reducer and effect with generated code, so it cannot
	/// look faster by registering less or by falling back to the scan.
	/// </summary>
	[WasmBenchmarkTheory]
	[Trait("Category", "WasmBenchmark")]
	[InlineData("small")]
	[InlineData("medium")]
	[InlineData("complex")]
	public async Task WhenAppStartsInTheBrowser_ThenGeneratedRegistrationIsFaster(string size)
	{
		SampleSize sampleSize = SampleSize.Parse(size);
		Variant[] variants = Variant.Builds.Select(x => new Variant(sampleSize.Name, x)).ToArray();

		(Dictionary<Variant, List<LoadResult>> loads, string environment) = await MeasureAsync(variants);
		string section = ResultsReport.CreateSection(sampleSize, variants, loads, WarmUpLoadsPerVariant, environment);
		Output.WriteLine(section);
		ResultsReport.Write(BenchmarkPaths.ResultsFilePath, sampleSize, section);

		AssertSameStore(sampleSize, variants, loads);
		AssertFaster(sampleSize, variants, loads, section);
	}

	/// <summary>
	/// Code that is not Fluxor's, in the browser: the complex sample with 0 and with 4,000 plain classes (no Fluxor
	/// code) in the same assembly, Scanned and Generated, the four variants taking turns. The reflection scan reads every
	/// type and method of the assembly, the generated registration only the Fluxor types. Reported in the results file,
	/// not a speed bar (the CoreCLR proof, ScalingTests in Fluxor.PerformanceTests, asserts it); the store of every
	/// variant is checked.
	/// </summary>
	[WasmBenchmarkTheory]
	[Trait("Category", "WasmBenchmark")]
	[InlineData("complex")]
	public async Task WhenComplexSampleGetsNonFluxorCode_ThenTheStartUpIsReported(string size)
	{
		SampleSize sampleSize = SampleSize.Parse(size);
		Variant[] variants = new[] { sampleSize.Name, sampleSize.Name + "Plain4000" }
			.SelectMany(x => Variant.Builds.Select(build => new Variant(x, build)))
			.ToArray();

		(Dictionary<Variant, List<LoadResult>> loads, string environment) = await MeasureAsync(variants);
		string section = ResultsReport.CreateNonFluxorCodeSection(variants, loads, WarmUpLoadsPerVariant, environment);
		Output.WriteLine(section);
		ResultsReport.Write(BenchmarkPaths.ResultsFilePath, ResultsReport.NonFluxorCodeKey, section);

		// The plain classes add no Fluxor code: every variant builds the complex sample's store
		AssertSameStore(sampleSize, variants, loads);
	}

	/// <summary>
	/// Publishes the variants, then loads each in headless Chromium: warm-up loads first, then the measured loads, the
	/// variants taking turns so drift on the machine affects every variant alike.
	/// </summary>
	private static async Task<(Dictionary<Variant, List<LoadResult>> Loads, string Environment)> MeasureAsync(Variant[] variants)
	{
		var roots = new Dictionary<Variant, string>();
		foreach (Variant variant in variants)
			roots[variant] = VariantPublisher.Publish(variant);

		var hosts = new Dictionary<Variant, StaticFileHost>();
		try
		{
			foreach (Variant variant in variants)
				hosts[variant] = await StaticFileHost.StartAsync(roots[variant]);

			await using BrowserSession browser = await BrowserSession.StartAsync();
			for (int warmUp = 0; warmUp < WarmUpLoadsPerVariant; warmUp++)
			{
				foreach (Variant variant in variants)
					await browser.LoadAsync(hosts[variant].Url);
			}

			var loads = variants.ToDictionary(x => x, _ => new List<LoadResult>());
			for (int load = 0; load < LoadsPerVariant; load++)
			{
				foreach (Variant variant in variants)
					loads[variant].Add(await browser.LoadAsync(hosts[variant].Url));
			}

			string environment = ResultsReport.DescribeEnvironment(browser.BrowserVersion, loads[variants[0]][0].Shape.Runtime);
			return (loads, environment);
		}
		finally
		{
			foreach (StaticFileHost host in hosts.Values)
				await host.DisposeAsync();
		}
	}

	private static void AssertSameStore(SampleSize size, Variant[] variants, Dictionary<Variant, List<LoadResult>> loads)
	{
		StoreShape first = loads[variants[0]][0].Shape;
		foreach (Variant variant in variants)
		{
			foreach (StoreShape shape in loads[variant].Select(x => x.Shape))
			{
				Assert.Equal(
					(size.Features, size.Reducers, size.Effects),
					(shape.Features, shape.Reducers, shape.Effects));
				Assert.Equal((first.MethodReducers, first.MethodEffects), (shape.MethodReducers, shape.MethodEffects));

				// The path the load really used: generated code created every method reducer and effect, or none
				Assert.Equal(variant.IsGenerated, shape.HasGeneratedRegistration);
				if (variant.IsGenerated)
					Assert.Equal((shape.MethodReducers, shape.MethodEffects), (shape.GeneratedReducers, shape.GeneratedEffects));
				else
					Assert.Equal((0, 0), (shape.GeneratedReducers, shape.GeneratedEffects));
			}
		}
	}

	private static void AssertFaster(SampleSize size, Variant[] variants, Dictionary<Variant, List<LoadResult>> loads, string section)
	{
		double scanned = ResultsReport.Median(loads[variants.Single(x => x.Build == Variant.Scanned)], x => x.Measured);
		double compact = ResultsReport.Median(loads[variants.Single(x => x.Build == Variant.Generated)], x => x.Measured);
		double ratio = compact / scanned;
		Assert.True(
			size.Passes(ratio),
			string.Create(
				CultureInfo.InvariantCulture,
				$"{size.Key}: generated (Compact) AddFluxor + store initialized {compact:0.00} ms is {ratio:0.00} x scanned"
				+ $" {scanned:0.00} ms; the bar is {size.DescribeBar()}")
			+ Environment.NewLine + section);
	}
}
