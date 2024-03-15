using Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverReducersWithActionInMethodSignatureTests.SupportFiles;

namespace Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverReducersWithActionInMethodSignatureTests.TypesThatShouldOnlyBeScannedExplicitly;

public static class ExplicitlyScannedReducers
{
	[ReducerMethod]
	public static TestState ReduceTestAction(TestState state, TestAction action) =>
		new TestState(counter: state.Counter + 1);
}
