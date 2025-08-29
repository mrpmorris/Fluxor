namespace FluxorBlazorWeb.ReduxDevToolsTutorial.Store.WeatherFeature;

public record FetchForecastsResultAction(IEnumerable<WeatherForecast> Forecasts);
