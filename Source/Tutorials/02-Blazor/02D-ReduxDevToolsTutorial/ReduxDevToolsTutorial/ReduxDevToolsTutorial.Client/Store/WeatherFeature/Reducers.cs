using Fluxor;
using System.Collections.Immutable;

namespace ReduxDevToolsTutorial.Client.Store.WeatherFeature;

public static class Reducers
{
	[ReducerMethod(typeof(FetchForecastsAction))]
	public static WeatherState ReduceFetchForecastsAction(WeatherState state) =>
		new WeatherState(IsLoading: true, Forecasts: []);

	[ReducerMethod]
	public static WeatherState ReduceFetchDataResultAction(WeatherState state, FetchForecastsResultAction action) =>
		new WeatherState(
		IsLoading: false,
		Forecasts: action.Forecasts?.ToImmutableList() ?? []);
}
