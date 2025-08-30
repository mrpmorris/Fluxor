using ReduxDevToolsTutorial.Contracts;

namespace ReduxDevToolsTutorial.Client.Store.WeatherFeature;

public record FetchForecastsResultAction(IEnumerable<WeatherForecast> Forecasts);
