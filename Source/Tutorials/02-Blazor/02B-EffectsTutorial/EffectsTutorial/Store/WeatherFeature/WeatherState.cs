using Fluxor;
using System.Collections.Immutable;

namespace FluxorBlazorWeb.EffectsTutorial.Store.WeatherFeature;

[FeatureState]
public record WeatherState(
	bool IsLoading,
	ImmutableList<WeatherForecast> Forecasts)
{
	public WeatherState() : this(
		IsLoading: false,
		Forecasts: [])
	{
	}
}
