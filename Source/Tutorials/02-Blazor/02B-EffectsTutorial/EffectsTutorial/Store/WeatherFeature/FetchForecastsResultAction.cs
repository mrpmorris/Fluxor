namespace FluxorBlazorWeb.EffectsTutorial.Store.WeatherFeature;

public record FetchForecastsResultAction(IEnumerable<WeatherForecast> Forecasts);
