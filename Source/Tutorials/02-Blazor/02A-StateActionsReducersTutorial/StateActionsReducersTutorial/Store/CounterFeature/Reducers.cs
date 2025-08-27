using Fluxor;

namespace FluxorBlazorWeb.StateActionsReducersTutorial.Store.CounterFeature;

public static class Reducers
{
	[ReducerMethod]
	public static CounterState Reduce(CounterState state, IncrementCounterAction action) =>
		new CounterState(ClickCount: state.ClickCount + 1);
}
