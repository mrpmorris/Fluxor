namespace Fluxor.UnitTests.DependencyInjectionTests.EffectDiscoveryTests.DiscoverEffectsWithActionInAttributeTests.SupportFiles
{
	public class TestFeature : Feature<TestState>
	{
		public override string GetName() => "TestEffects";
		protected override TestState GetInitialState() =>
			new TestState(0);
	}
}
