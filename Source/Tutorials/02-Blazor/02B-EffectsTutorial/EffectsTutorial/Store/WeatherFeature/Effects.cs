using Fluxor;
using System.Collections.Immutable;

namespace FluxorBlazorWeb.EffectsTutorial.Store.WeatherFeature;

public class Effects
{
	private readonly static string[] Summaries =
		["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

	[EffectMethod(typeof(FetchDataAction))]
	public async Task HandleFetchDataAction(IDispatcher dispatcher)
	{
		// Simulate a delay
		await Task.Delay(500);

		DateOnly startDate = DateOnly.FromDateTime(DateTime.Now);
		var forecasts =
			Enumerable.Range(1, 5)
			.Select(
				index => new WeatherForecast(
					Date: startDate.AddDays(index),
					TemperatureC: Random.Shared.Next(-20, 55),
					Summary: Summaries[Random.Shared.Next(Summaries.Length)]
				)
			);

		var action = new FetchDataResultAction(forecasts);
		dispatcher.Dispatch(action);
	}
}
