using Microsoft.Playwright;

namespace Fluxor.Performance.BlazorWasm.Benchmark;

/// <summary>
/// Headless Chromium. Every load gets a new browser context, so nothing is cached between loads.
/// </summary>
internal sealed class BrowserSession : IAsyncDisposable
{
	private const float ReadyTimeoutMilliseconds = 180_000;

	// Collected in the page after the app reports done; longtask entries still queued are taken from the observer
	private const string ReadScript = """
		() => {
			const marks = {};
			for (const entry of performance.getEntriesByType('mark'))
				marks[entry.name] = entry.startTime;
			const bench = window.fluxorBench;
			for (const entry of bench.observer.takeRecords())
				bench.longTasks.push({ start: entry.startTime, duration: entry.duration });
			return JSON.stringify({ marks, longTasks: bench.longTasks, shape: bench.shape });
		}
		""";

	private readonly IPlaywright Playwright;
	private readonly IBrowser Browser;

	public string BrowserVersion => Browser.Version;

	private BrowserSession(IPlaywright playwright, IBrowser browser)
	{
		Playwright = playwright;
		Browser = browser;
	}

	public static async Task<BrowserSession> StartAsync()
	{
		IPlaywright playwright = await Microsoft.Playwright.Playwright.CreateAsync();
		IBrowser browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
		return new BrowserSession(playwright, browser);
	}

	public async Task<LoadResult> LoadAsync(string url)
	{
		await using IBrowserContext context = await Browser.NewContextAsync();
		IPage page = await context.NewPageAsync();
		var errors = new List<string>();
		page.PageError += (_, error) => errors.Add(error);
		page.Console += (_, message) =>
		{
			if (message.Type == "error")
				errors.Add(message.Text);
		};

		await page.GotoAsync(url);
		try
		{
			await page.WaitForFunctionAsync(
				"() => window.fluxorBench && window.fluxorBench.done",
				null,
				new PageWaitForFunctionOptions { Timeout = ReadyTimeoutMilliseconds, PollingInterval = 50 });
		}
		catch (TimeoutException exception)
		{
			throw new TimeoutException($"{url} did not report done: {string.Join(Environment.NewLine, errors)}", exception);
		}

		string json = await page.EvaluateAsync<string>(ReadScript);
		return LoadResult.Parse(json);
	}

	public async ValueTask DisposeAsync()
	{
		await Browser.DisposeAsync();
		Playwright.Dispose();
	}
}
