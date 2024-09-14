﻿namespace Fluxor.UnitTests.StoreTests.ThreadingTests.DispatchTests.SupportFiles;

class IncrementCounterReducer : Reducer<CounterState, IncrementCounterAction>
{
	public override CounterState Reduce(CounterState state, IncrementCounterAction action) =>
		new(state.Counter + 1);
}
