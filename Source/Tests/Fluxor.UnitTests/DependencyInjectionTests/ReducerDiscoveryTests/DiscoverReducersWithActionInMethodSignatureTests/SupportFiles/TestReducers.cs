namespace Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverReducersWithActionInMethodSignatureTests.SupportFiles;

public static class TestReducers
{
	[ReducerMethod]
	public static TestState ReduceTestAction(TestState state, TestAction action) =>
		new(counter: state.Counter + 1);
}
