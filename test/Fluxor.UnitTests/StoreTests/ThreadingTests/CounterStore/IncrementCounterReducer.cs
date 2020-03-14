namespace Fluxor.UnitTests.StoreTests.ThreadingTests.CounterStore
{
	class IncrementCounterReducer : Reducer<CounterState, IncrementCounterAction>
	{
		public override CounterState Reduce(CounterState state, IncrementCounterAction action) =>
			new CounterState(state.Counter + 1);
	}
}
