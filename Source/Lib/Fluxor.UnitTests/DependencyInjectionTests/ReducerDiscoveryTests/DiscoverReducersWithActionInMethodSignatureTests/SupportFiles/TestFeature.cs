namespace Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverReducersWithActionInMethodSignatureTests.SupportFiles;

public class TestFeature : Feature<TestState>
{
	public override string GetName() => "Test";
	protected override TestState GetInitialState() => new(counter: 0);
}
