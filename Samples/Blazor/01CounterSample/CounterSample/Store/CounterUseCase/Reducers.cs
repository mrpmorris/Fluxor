using Fluxor;

namespace CounterSample.Store.CounterUseCase
{
	public static class Reducers
	{
		[ReducerMethod]
		public static State ReduceIncrementCounterAction(State state, IncrementCounterAction action) =>
			new State(clickCount: state.ClickCount + 1);
	}
}
