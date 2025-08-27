using Fluxor;
using System.Collections.Immutable;

namespace FluxorBlazorWeb.EffectsTutorial.Store.WeatherFeature;

public static class Reducers
{
	[ReducerMethod(typeof(FetchDataAction))]
	public static WeatherState ReduceFetchDataAction(WeatherState state) =>
		new WeatherState(IsLoading: true, Forecasts: []);

	[ReducerMethod]
	public static WeatherState ReduceFetchDataResultAction(WeatherState state, FetchDataResultAction action) =>
	  new WeatherState(
		IsLoading: false,
		Forecasts: action.Forecasts?.ToImmutableList() ?? []);

}
