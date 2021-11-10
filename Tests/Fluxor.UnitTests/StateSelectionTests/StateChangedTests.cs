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
			Subject.Select(x => x[0]); // TODO: Move 1 line down to ensure you can subscribe before calling Select
			Subject.StateChanged += (_, _) => subject1InvokeCount++;

			int subject2InvokeCount = 0;
			var subject2 = new StateSelection<string, char>(MockFeature.Object);
			subject2.Select(x => x[1]);
			subject2.StateChanged += (_, _) => subject2InvokeCount++;

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
			Subject.Select(x => x[0]);
			Subject.StateChanged += (_, _) => invokeCount++;

			FeatureState = "ABC";
			MockFeature.Raise(x => x.StateChanged += null, EventArgs.Empty);

			FeatureState = "AYZ";
			MockFeature.Raise(x => x.StateChanged += null, EventArgs.Empty);

			Assert.Equal(1, invokeCount);
		}

	}
}
