namespace Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverGenericReducersWithActionInMethodSignatureTests.SupportFiles
{
	public class TestFeature : Feature<TestState<int>>
	{
		public override string GetName() => "Test";
		protected override TestState<int> GetInitialState() =>
			new TestState<int>(new int[] { 1, 2, 3, 5, 8, 13 });
	}
}
