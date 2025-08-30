namespace ReduxDevToolsTutorial.Contracts;

public interface IWeatherService
{
	Task<WeatherForecast[]> GetForecastsAsync();
}
