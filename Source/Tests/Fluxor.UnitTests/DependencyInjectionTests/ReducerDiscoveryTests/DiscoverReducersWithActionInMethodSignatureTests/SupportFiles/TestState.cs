namespace Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverReducersWithActionInMethodSignatureTests.SupportFiles;

public class TestState
{
	public int Counter { get; private set; }

	public TestState(int counter)
	{
		Counter = counter;
	}
}
