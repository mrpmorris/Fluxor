namespace Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverGenericReducerClassesTests.SupportFiles
{
	public class TestFeature : Feature<TestState>
	{
		public override string GetName() => "Test";
		protected override TestState GetInitialState() => new TestState(reducerWasExecuted: false);
	}
}
