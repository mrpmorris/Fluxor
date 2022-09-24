using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.UnitTests.FeatureTests
{
	public class FeatureTests
	{
		private class TestState
		{
			public int SomeValue { get; }

			public TestState(int someValue)
			{
				SomeValue = someValue;
			}
		}
		

		private class SomeFeature : Feature<TestState>
		{
			public override string GetName() => "SomeName";

			protected override TestState GetInitialState() => new TestState(1);
		}

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
