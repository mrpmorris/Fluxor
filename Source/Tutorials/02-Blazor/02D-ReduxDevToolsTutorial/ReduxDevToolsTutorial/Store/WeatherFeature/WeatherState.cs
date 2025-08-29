using Fluxor;
using System.Collections.Immutable;

namespace FluxorBlazorWeb.ReduxDevToolsTutorial.Store.WeatherFeature;

[FeatureState]
public record WeatherState(
	bool IsLoading,
	ImmutableList<WeatherForecast> Forecasts)
{
	// Parameterless constructor required for creating initial state
	public WeatherState() : this(
		IsLoading: false,
		Forecasts: [])
	{
	}
}
