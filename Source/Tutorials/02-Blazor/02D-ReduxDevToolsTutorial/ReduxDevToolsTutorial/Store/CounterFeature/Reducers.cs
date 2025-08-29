using Fluxor;

namespace FluxorBlazorWeb.ReduxDevToolsTutorial.Store.CounterFeature;

public static class Reducers
{
	[ReducerMethod(typeof(IncrementCounterAction))]
	public static CounterState Reduce(CounterState state) =>
		new CounterState(ClickCount: state.ClickCount + 1);
}
