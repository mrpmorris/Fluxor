using Fluxor;
using Fluxor.Blazor.Web.Middlewares.Routing;
using ReduxDevToolsTutorial.Contracts;

namespace ReduxDevToolsTutorial.Client.Store.WeatherFeature;

public class Effects
{
	private readonly static string[] Summaries =
		["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

	[EffectMethod]
	public Task HandleGoAction(GoAction goAction, IDispatcher dispatcher)
	{
		Console.WriteLine("Go to: " + goAction.NewUri);
		if (new Uri(goAction.NewUri).AbsolutePath.Equals("/Weather", StringComparison.InvariantCultureIgnoreCase))
		{
			var action = new FetchForecastsAction();
			dispatcher.Dispatch(action);
		}
		return Task.CompletedTask;
	}


	[EffectMethod(typeof(FetchForecastsAction))]
	public async Task HandleFetchDataAction(IDispatcher dispatcher)
	{
		Console.WriteLine("Simulating a delay for fetching weather forecasts");
		// Simulate a delay
		await Task.Delay(100);

		var startDate = DateOnly.FromDateTime(DateTime.Now);
		var forecasts =
			Enumerable.Range(1, 5)
			.Select(
				index => new WeatherForecast(
					Date: startDate.AddDays(index),
					TemperatureC: Random.Shared.Next(-20, 55),
					Summary: Summaries[Random.Shared.Next(Summaries.Length)]
				)
			);

		Console.WriteLine("Received weather forecasts, updating state");
		var action = new FetchForecastsResultAction(forecasts);
		dispatcher.Dispatch(action);
	}
}
