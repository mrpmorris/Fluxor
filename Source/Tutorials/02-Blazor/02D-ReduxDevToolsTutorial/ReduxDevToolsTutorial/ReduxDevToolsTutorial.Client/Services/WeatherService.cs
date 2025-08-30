using ReduxDevToolsTutorial.Contracts;
using System.Net.Http.Json;

namespace ReduxDevToolsTutorial.Client.Services;

public class WeatherService : IWeatherService
{
	private readonly IHttpClientFactory HttpClientFactory;

	public WeatherService(IHttpClientFactory httpClientFactory)
	{
		HttpClientFactory = httpClientFactory;
	}

	public async Task<WeatherForecast[]> GetForecastsAsync()
	{
		// Get an HttpClient
		HttpClient httpClient = HttpClientFactory.CreateClient("Api");

		// Call the API
		WeatherForecast[]? response = await httpClient.GetFromJsonAsync<WeatherForecast[]>("/Api/Weather");

		// Return a non-null response
		return response ?? [];
	}
}
