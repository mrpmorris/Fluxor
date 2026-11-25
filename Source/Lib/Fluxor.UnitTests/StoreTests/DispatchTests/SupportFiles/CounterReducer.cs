using System;

namespace Fluxor.UnitTests.StoreTests.DispatchTests.SupportFiles;

public class CounterReducer : IReducer<int>
{
	public bool ShouldReduceStateForAction(object action) => action is Boom or Increment;

	public int Reduce(int state, object action) =>
		action is Boom
			? throw new InvalidOperationException("boom")
			: state + 1;
}
