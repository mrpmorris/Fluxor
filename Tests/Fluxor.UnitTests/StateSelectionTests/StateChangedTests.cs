using System;
using Xunit;

namespace Fluxor.UnitTests.StateSelectionTests
{
	public  class StateChangedTests : TestsBase
	{
		[Fact]
		public void WhenFeatureStateChanges_AndSelectedValueHasChanged_ThenTriggersStateChanged()
		{
			int subject1InvokeCount = 0;
			Subject.StateChanged += (_, _) => subject1InvokeCount++;
			Subject.Select(x => x[0]);

			int subject2InvokeCount = 0;
			var subject2 = new StateSelection<string, char>(MockFeature.Object);
			subject2.StateChanged += (_, _) => subject2InvokeCount++;
			subject2.Select(x => x[1]);

			FeatureState = "ABC";
			MockFeature.Raise(x => x.StateChanged += null, EventArgs.Empty);
			Assert.Equal(1, subject1InvokeCount);
			Assert.Equal(1, subject2InvokeCount);

			FeatureState = "BBC";
			MockFeature.Raise(x => x.StateChanged += null, EventArgs.Empty);
			Assert.Equal(2, subject1InvokeCount);
			Assert.Equal(1, subject2InvokeCount);

			FeatureState = "BYZ";
			MockFeature.Raise(x => x.StateChanged += null, EventArgs.Empty);
			Assert.Equal(2, subject1InvokeCount);
			Assert.Equal(2, subject2InvokeCount);

			FeatureState = "BYE";
			MockFeature.Raise(x => x.StateChanged += null, EventArgs.Empty);
			Assert.Equal(2, subject1InvokeCount);
			Assert.Equal(2, subject2InvokeCount);
		}

		[Fact]
		public void WhenFeatureStateChanges_AndSelectedValueIsTheSame_ThenDoesNotTriggerStateChanged()
		{
			int invokeCount = 0;
			Subject.StateChanged += (_, _) => invokeCount++;
			Subject.Select(x => x[0]);

			FeatureState = "ABC";
			MockFeature.Raise(x => x.StateChanged += null, EventArgs.Empty);

			FeatureState = "AYZ";
			MockFeature.Raise(x => x.StateChanged += null, EventArgs.Empty);

			Assert.Equal(1, invokeCount);
		}

		[Fact]
		public void WhenValueComparerSaysValueHasNotChanged_ThenDoesNotTriggerStateChanged()
		{
			int invokeCount = 0;
			Subject.StateChanged += (_, _) => invokeCount++;
			Subject.Select(x => x[0], valueEquals: (x, y) => x == '*' || x == y);

			FeatureState = "ABC";
			MockFeature.Raise(x => x.StateChanged += null, EventArgs.Empty);
			Assert.Equal(1, invokeCount);

			FeatureState = "XYZ";
			MockFeature.Raise(x => x.StateChanged += null, EventArgs.Empty);
			Assert.Equal(2, invokeCount);

			FeatureState = "*23";
			MockFeature.Raise(x => x.StateChanged += null, EventArgs.Empty);
			Assert.Equal(2, invokeCount);
		}

		[Fact]
		public void WhenUserSetsSelector_ThenPreviousValueIsRetrievedFromState()
		{
			FeatureState = "ABC";
			int invokeCount = 0;
			Subject.StateChanged += (_, _) => invokeCount++;
			Subject.Select(
				selector: x => x is null ? null : x[0], 
				valueEquals: (x, y) => x == '*' || x == y);

			FeatureState = null;
			MockFeature.Raise(x => x.StateChanged += null, EventArgs.Empty);
			Assert.Equal(1, invokeCount);

		}
	}
}
