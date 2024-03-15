namespace Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverReducersWithActionInAttributeTests.SupportFiles;

public class TestState
{
	public int Counter { get; }

	public TestState(int counter)
	{
		Counter = counter;
	}
}
