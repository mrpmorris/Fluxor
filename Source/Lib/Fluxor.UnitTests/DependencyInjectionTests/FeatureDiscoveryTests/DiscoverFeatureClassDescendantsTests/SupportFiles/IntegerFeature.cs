namespace Fluxor.UnitTests.DependencyInjectionTests.FeatureDiscoveryTests.DiscoverFeatureClassDescendantsTests.SupportFiles;

public class IntegerFeature : Feature<int>
{
	public override string GetName() => nameof(IntegerFeature);

	protected override int GetInitialState() => 42;
}
