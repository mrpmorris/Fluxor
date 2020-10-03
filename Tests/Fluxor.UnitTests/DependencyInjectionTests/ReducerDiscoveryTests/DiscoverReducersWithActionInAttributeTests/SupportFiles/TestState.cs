namespace Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverReducersWithActionInAttributeTests.SupportFiles
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
