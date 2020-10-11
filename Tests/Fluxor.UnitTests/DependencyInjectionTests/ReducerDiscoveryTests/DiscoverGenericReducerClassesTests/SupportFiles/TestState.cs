namespace Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverGenericReducerClassesTests.SupportFiles
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
