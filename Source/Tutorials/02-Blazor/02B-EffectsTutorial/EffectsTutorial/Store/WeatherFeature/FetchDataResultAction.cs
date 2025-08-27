namespace FluxorBlazorWeb.EffectsTutorial.Store.WeatherFeature;

public record FetchDataResultAction(IEnumerable<WeatherForecast> Forecasts);
