namespace Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverReducersWithActionInMethodSignatureTests.SupportFiles
{
	public class TestState
	{
		public bool ReducerWasExecuted { get; private set; }

		public TestState(bool reducerWasExecuted)
		{
			ReducerWasExecuted = reducerWasExecuted;
		}
	}
}
