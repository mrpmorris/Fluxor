namespace Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverReducersWithActionInAttributeTests.SupportFiles
{
	public static class TestReducers
	{
		[ReducerMethod]
		public static TestState ReduceTestAction(TestState state, TestAction action) =>
			new TestState(reducerWasExecuted: true);
	}
}
