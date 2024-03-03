using Xunit;
using Fluxor.UnitTests.FeatureTests.SupportFiles;

namespace Fluxor.UnitTests.FeatureTests
{
	public partial class FeatureTests
	{
		[Fact]
		public void WhenFeatureRestoredStateIsCalledBeforeGettingFeatureState_StateReturnsRestoredState()
		{
			IFeature<TestState> feature = new SomeFeature();
			feature.RestoreState(new TestState(2));
			TestState featureState = feature.State;
			Assert.Equal(2, featureState.SomeValue);
		}

		[Fact]
		public void WhenFeatureRestoredStateIsCalledAfterGettingFeatureState_StateReturnsRestoredState()
		{
			IFeature<TestState> feature = new SomeFeature();
			TestState featureState = feature.State;
			feature.RestoreState(new TestState(2));
			featureState = feature.State;
			Assert.Equal(2, featureState.SomeValue);
		}

		[Fact]
		public void WhenGettingFeatureStateAfterConstruction_StateReturnsInitialState()
		{
			IFeature<TestState> feature = new SomeFeature();
			TestState featureState = feature.State;
			Assert.Equal(1, featureState.SomeValue);
		}
	}
}
