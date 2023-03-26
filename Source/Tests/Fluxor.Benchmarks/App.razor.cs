using Fluxor.Benchmarks.Benchmarks;

namespace Fluxor.Benchmarks;

public partial class App
{
	private readonly List<string> Results = new();

	private async Task RunBenchmarksAsync()
	{
		await LogOutput("Running");
		for (int i = 0; i < 5; i++)
		{
			await ScanTypesBenchmark.ExecuteAsync(LogOutput);
			await ScanAssembliesBenchmark.ExecuteAsync(LogOutput);
		}
	}

	private async Task LogOutput(string text)
	{
		Results.Add(text);
		StateHasChanged();
		await Task.Delay(1);
	}


}

