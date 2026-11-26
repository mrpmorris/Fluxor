using System.Reflection;
using Fluxor.Performance.BlazorWasm;
using Microsoft.Extensions.DependencyInjection;

namespace Fluxor.PerformanceTests;

/// <summary>
/// The samples the browser benchmark compares, built two ways each (scanned and generated), must create the same
/// store, with every method reducer and effect of the generated build created by generated code. Checked here on
/// CoreCLR on every build, so the browser benchmark only measures speed.
/// </summary>
public class StoreShapeTests
{
	private static readonly string[] Builds = ["Scanned", "Generated"];

	[Theory]
	[InlineData("Small", 20, 100, 100)]
	[InlineData("Medium", 80, 600, 600)]
	[InlineData("Complex", 200, 2_000, 2_000)]
	public async Task WhenASizeSampleIsBuiltBothWays_ThenBothBuildsCreateTheSameStore(
		string size,
		int features,
		int reducers,
		int effects)
	{
		var shapes = new Dictionary<string, StoreShape>();
		foreach (string build in Builds)
		{
			Assembly assembly = Assembly.Load($"Fluxor.Performance.Sample.{size}.{build}");
			var services = new ServiceCollection();
			services.AddFluxor(options => options.ScanAssemblies(assembly));
			await using ServiceProvider serviceProvider = services.BuildServiceProvider();
			IStore store = serviceProvider.GetRequiredService<IStore>();
			await store.InitializeAsync();
			shapes[build] = StoreShape.Read(store, assembly);
		}

		StoreShape scanned = shapes["Scanned"];
		Assert.Equal((features, reducers, effects), (scanned.Features, scanned.Reducers, scanned.Effects));
		Assert.False(scanned.HasGeneratedRegistration);
		Assert.Equal((0, 0), (scanned.GeneratedReducers, scanned.GeneratedEffects));
		foreach (string build in Builds.Where(x => x != "Scanned"))
		{
			StoreShape generated = shapes[build];
			Assert.Equal(scanned with { HasGeneratedRegistration = true, GeneratedReducers = 0, GeneratedEffects = 0 },
				generated with { GeneratedReducers = 0, GeneratedEffects = 0 });
			// No silent fallback to the scan: generated code created every method reducer and effect
			Assert.Equal((generated.MethodReducers, generated.MethodEffects), (generated.GeneratedReducers, generated.GeneratedEffects));
		}
	}
}
