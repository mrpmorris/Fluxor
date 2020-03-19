using Fluxor;

namespace FluxorBlazorWeb.StateActionsReducersSample.Store.CounterUseCase
{
	public static class Reducers
	{
		[ReducerMethod]
		public static CounterState ReduceIncrementCounterAction(CounterState state, IncrementCounterAction action) =>
			new CounterState(clickCount: state.ClickCount + 1);
	}
}
