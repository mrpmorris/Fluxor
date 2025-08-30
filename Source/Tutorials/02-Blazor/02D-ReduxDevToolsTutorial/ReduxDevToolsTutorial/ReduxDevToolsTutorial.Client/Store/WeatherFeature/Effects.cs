using Fluxor;
using ReduxDevToolsTutorial.Contracts;

namespace ReduxDevToolsTutorial.Client.Store.WeatherFeature;

public class Effects
{
	private readonly static string[] Summaries =
		["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

	[EffectMethod(typeof(FetchForecastsAction))]
	public async Task HandleFetchDataAction(IDispatcher dispatcher)
	{
		// Simulate a delay
		await Task.Delay(1_000);

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

		var action = new FetchForecastsResultAction(forecasts);
		dispatcher.Dispatch(action);
	}
}
