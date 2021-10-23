namespace Fluxor.UnitTests.DependencyInjectionTests.FeatureDiscoveryTests.SupportFiles
{
	public class StringFeature : Feature<string>
	{
		public override string GetName() => nameof(StringFeature);
		protected override string GetInitialState() => "InitialState";
	}
}
