using Fluxor;

namespace EffectsSample.Client.Store.WeatherUseCase
{
	public static class Reducers
	{
		[ReducerMethod]
		public static State ReduceFetchDataAction(State state, FetchDataAction action) =>
			new State(
				isLoading: true,
				forecasts: null);

		[ReducerMethod]
		public static State ReduceFetchDataResultAction(State state, FetchDataResultAction action) =>
			new State(
				isLoading: false,
				forecasts: action.Forecasts);
	}
}
