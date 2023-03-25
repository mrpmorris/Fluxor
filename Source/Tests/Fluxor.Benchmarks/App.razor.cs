using System.Diagnostics;

namespace Fluxor.Benchmarks;

[CreateStoreClasses]
public partial class App
{
	private readonly List<string> Results = new();

	private async Task RunBenchmarksAsync()
	{
		Results.Add("Started scan");
		StateHasChanged();
		await Task.Yield();
		
		var services = new ServiceCollection();
		
		var stopwatch = Stopwatch.StartNew();
		services.AddFluxor(x => x.ScanAssemblies(typeof(App).Assembly));
		stopwatch.Stop();

		Results.Add($"Scan took {stopwatch.ElapsedMilliseconds} ms");
		StateHasChanged();
		await Task.Yield();

		Results.Add("Starting InitializeAsync");
		StateHasChanged();
		await Task.Yield();

		IServiceProvider serviceProvider = services.BuildServiceProvider();
		IDispatcher dispatcher = serviceProvider.GetService<IDispatcher>();
		IStore store = serviceProvider.GetService<IStore>();

		stopwatch.Restart();
		await store.InitializeAsync();
		stopwatch.Stop();

		Results.Add($"InitializeAsync took {stopwatch.ElapsedMilliseconds} ms");
	}
}

