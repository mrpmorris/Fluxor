namespace Fluxor.UnitTests.FeatureTests.SupportFiles
{
	public class SomeFeature : Feature<TestState>
	{
		public override string GetName() => "SomeName";

		protected override TestState GetInitialState() => new TestState(1);
	}
}
