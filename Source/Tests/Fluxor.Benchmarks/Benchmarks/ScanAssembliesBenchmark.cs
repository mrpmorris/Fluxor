using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Fluxor.Benchmarks.Benchmarks;

public class ScanAssembliesBenchmark
{
	[Benchmark]
	public void ScanAssemblies()
	{
		var services = new ServiceCollection();
		services.AddFluxor(x => x.ScanAssemblies(typeof(Program).Assembly));
	}
}
