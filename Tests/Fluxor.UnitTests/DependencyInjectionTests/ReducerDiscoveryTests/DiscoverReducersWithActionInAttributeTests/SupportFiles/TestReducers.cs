namespace Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverReducersWithActionInAttributeTests.SupportFiles
{
	public static class TestReducers
	{
		[ReducerMethod(typeof(TestAction))]
		public static TestState ReduceTestAction(TestState state) =>
			new TestState(reducerWasExecuted: true);
	}
}
