using ReduxDevToolsTutorial.Contracts;

namespace ReduxDevToolsTutorial.Services;

// Refactored code from Client\Pages\Weather.razor
public class WeatherService : IWeatherService
{
	private readonly static string[] Summaries = [
		"Freezing", "Bracing", "Chilly", "Cool", "Mild",
		"Warm", "Balmy", "Hot", "Sweltering", "Scorching"
	];

	public async Task<WeatherForecast[]> GetForecastsAsync()
	{
		await Task.Delay(1000); // Simulate slowness
		DateOnly startDate = DateOnly.FromDateTime(DateTime.Now);
		return Enumerable
			.Range(1, 5)
			.Select(index =>
				new WeatherForecast(
					Date: startDate.AddDays(index),
					TemperatureC: Random.Shared.Next(-20, 55),
					Summary: Summaries[Random.Shared.Next(Summaries.Length)]
				)
			)
			.ToArray();
	}
}
