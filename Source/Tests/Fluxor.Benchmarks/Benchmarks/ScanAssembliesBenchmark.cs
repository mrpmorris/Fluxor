using System.Diagnostics;

namespace Fluxor.Benchmarks.Benchmarks;

public static class ScanAssembliesBenchmark
{
	public async static Task ExecuteAsync(Func<string, Task> log)
	{
		var services = new ServiceCollection();

		var stopwatch = Stopwatch.StartNew();
		services.AddFluxor(x => x.ScanAssemblies(typeof(App).Assembly));
		stopwatch.Stop();

		await log($"ScanAssemblies took {stopwatch.ElapsedMilliseconds} ms");
	}
}
