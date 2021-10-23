namespace Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverGenericReducerClassesTests.SupportFiles
{
	public class TestState
	{
		public int Count { get; private set; }

		public TestState(int count)
		{
			Count = count;
		}
	}
}
