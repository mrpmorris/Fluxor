namespace Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverReducersWithActionInAttributeTests.SupportFiles
{
	public class TestFeature : Feature<TestState>
	{
		public override string GetName() => "Test";
		protected override TestState GetInitialState() => new TestState(counter: 0);
	}
}
