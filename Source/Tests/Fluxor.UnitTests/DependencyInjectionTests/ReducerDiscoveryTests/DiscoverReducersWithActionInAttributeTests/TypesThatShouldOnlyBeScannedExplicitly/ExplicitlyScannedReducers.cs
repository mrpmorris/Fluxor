using Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverReducersWithActionInAttributeTests.SupportFiles;

namespace Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverReducersWithActionInAttributeTests.TypesThatShouldOnlyBeScannedExplicitly;

public static class ExplicitlyScannedReducers
{
	[ReducerMethod(typeof(TestAction))]
	public static TestState ReduceTestAction(TestState state) => new(counter: state.Counter + 1);
}
