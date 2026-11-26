using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Microsoft.Extensions.DependencyInjection;

namespace Fluxor.Benchmarks;

/// <summary>
/// Cold start of Fluxor with the standard sample (200 states, 2,000 reducers, 2,000 effects), registered by
/// generated code or scanned by reflection: AddFluxor, build the provider, resolve the store.
/// Run on demand, not in CI: <c>dotnet run -c Release --project Source/Performance/Fluxor.Benchmarks</c>
/// </summary>
[SimpleJob(RunStrategy.ColdStart, launchCount: 10)]
[MemoryDiagnoser]
public class StartupBenchmarks
{
	private static readonly Assembly ScannedAssembly = Assembly.Load("Fluxor.Performance.Sample.Scanned");
	private static readonly Assembly GeneratedAssembly = Assembly.Load("Fluxor.Performance.Sample.Generated");

	[Benchmark]
	public IStore Scanned_AddFluxorAndBuildStore() => AddFluxorAndBuildStore(ScannedAssembly);

	[Benchmark]
	public IStore Generated_AddFluxorAndBuildStore() => AddFluxorAndBuildStore(GeneratedAssembly);

	private static IStore AddFluxorAndBuildStore(Assembly assembly)
	{
		var services = new ServiceCollection();
		services.AddFluxor(options => options.ScanAssemblies(assembly));
		ServiceProvider serviceProvider = services.BuildServiceProvider();
		return serviceProvider.GetRequiredService<IStore>();
	}
}
