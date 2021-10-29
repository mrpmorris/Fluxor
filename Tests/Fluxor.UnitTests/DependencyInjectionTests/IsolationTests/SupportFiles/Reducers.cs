namespace Fluxor.UnitTests.DependencyInjectionTests.IsolationTests.SupportFiles
{
	public static class Reducers
	{
		[ReducerMethod(typeof(IncrementCounterAction))]
		public static CounterState ReduceIncrementCounterAction(CounterState state) =>
			new CounterState(state.Counter + 1);
	}
}
