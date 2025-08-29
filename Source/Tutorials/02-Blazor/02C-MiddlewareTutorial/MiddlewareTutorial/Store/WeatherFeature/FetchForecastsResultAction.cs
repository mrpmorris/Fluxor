namespace FluxorBlazorWeb.MiddlewareTutorial.Store.WeatherFeature;

public record FetchForecastsResultAction(IEnumerable<WeatherForecast> Forecasts);
