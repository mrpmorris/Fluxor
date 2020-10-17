namespace Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverGenericReducerClassesTests.SupportFiles
{
	public class TestGenericReducerClass : AbstractTestGenericReducerClass<TestAction>
	{
	}

	public abstract class AbstractTestGenericReducerClass<TAction> : Reducer<TestState, TAction>
	{
		public override TestState Reduce(TestState state, TAction action) =>
			new TestState(reducerWasExecuted: true);
	}
}
