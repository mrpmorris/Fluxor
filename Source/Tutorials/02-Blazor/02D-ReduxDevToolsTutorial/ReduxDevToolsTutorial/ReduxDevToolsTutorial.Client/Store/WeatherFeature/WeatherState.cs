using Fluxor;
using ReduxDevToolsTutorial.Contracts;
using System.Collections.Immutable;

namespace ReduxDevToolsTutorial.Client.Store.WeatherFeature;

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
