using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace Fluxor.UnitTests.StateSelectionTests;

public class SelectedValueChangedTests : TestsBase
{
	private void ChangeFeatureState(string newState)
	{
		FeatureState = newState;
		MockFeature.Raise(x => x.StateChanged += null, EventArgs.Empty);
	}

	// Bug: ShouldBeSubscribedToFeature ignores the callback passed to Select, so without an event subscriber the
	// selection never subscribes to the feature and the callback never runs (https://github.com/mrpmorris/Fluxor/issues/586).
	// Fixing it needs a decision on how the selection unsubscribes, because IStateSelection has no Dispose.
	[Fact(Skip = "Known bug #586: Select's selectedValueChanged callback alone never fires; fix pending a design decision")]
	public void WhenOnlyASelectedValueChangedCallbackIsPassedToSelect_ThenTheCallbackIsInvokedWhenTheValueChanges()
	{
		var receivedValues = new List<char?>();
		Subject.Select(x => x[0], selectedValueChanged: receivedValues.Add);

		ChangeFeatureState("ABC");

		Assert.Equal(new char?[] { 'A' }, receivedValues);
	}

	[Fact]
	public void WhenSelectedValueChangedIsSubscribed_ThenItIsRaisedWithTheNewValue()
	{
		var receivedValues = new List<char?>();
		Subject.SelectedValueChanged += (sender, value) =>
		{
			Assert.Same(Subject, sender);
			receivedValues.Add(value);
		};
		Subject.Select(x => x[0]);

		ChangeFeatureState("ABC");
		ChangeFeatureState("AXX");
		ChangeFeatureState("BXX");

		Assert.Equal(new char?[] { 'A', 'B' }, receivedValues);
	}

	[Fact]
	public void WhenTheFirstSubscriberSubscribes_ThenSubscribesToTheFeatureOnce()
	{
		Subject.SelectedValueChanged += (_, _) => { };
		Subject.StateChanged += (_, _) => { };

		MockFeature.VerifyAdd(x => x.StateChanged += It.IsAny<EventHandler>(), Times.Once());
	}

	[Fact]
	public void WhenTheLastSubscriberUnsubscribes_ThenUnsubscribesFromTheFeature()
	{
		EventHandler<char?> selectedValueChanged = (_, _) => { };
		EventHandler stateChanged = (_, _) => { };
		Subject.SelectedValueChanged += selectedValueChanged;
		Subject.StateChanged += stateChanged;

		Subject.SelectedValueChanged -= selectedValueChanged;
		MockFeature.VerifyRemove(x => x.StateChanged -= It.IsAny<EventHandler>(), Times.Never());

		Subject.StateChanged -= stateChanged;
		MockFeature.VerifyRemove(x => x.StateChanged -= It.IsAny<EventHandler>(), Times.Once());
	}

	[Fact]
	public void WhenTheOnlySelectedValueChangedSubscriberUnsubscribes_ThenUnsubscribesFromTheFeature()
	{
		EventHandler<char?> selectedValueChanged = (_, _) => { };
		Subject.SelectedValueChanged += selectedValueChanged;

		Subject.SelectedValueChanged -= selectedValueChanged;

		MockFeature.VerifyRemove(x => x.StateChanged -= It.IsAny<EventHandler>(), Times.Once());
	}

	[Fact]
	public void WhenOneOfTwoSubscribersUnsubscribes_ThenTheOtherIsStillNotified()
	{
		EventHandler<char?> selectedValueChanged = (_, _) => { };
		int stateChangedCount = 0;
		Subject.SelectedValueChanged += selectedValueChanged;
		Subject.StateChanged += (_, _) => stateChangedCount++;
		Subject.Select(x => x[0]);

		Subject.SelectedValueChanged -= selectedValueChanged;
		ChangeFeatureState("ABC");

		Assert.Equal(1, stateChangedCount);
	}

	[Fact]
	public void WhenStateChangedIsUnsubscribed_ThenItIsNoLongerRaised()
	{
		int stateChangedCount = 0;
		EventHandler stateChanged = (_, _) => stateChangedCount++;
		Subject.StateChanged += stateChanged;
		Subject.Select(x => x[0]);

		ChangeFeatureState("ABC");
		Subject.StateChanged -= stateChanged;
		ChangeFeatureState("BBC");

		Assert.Equal(1, stateChangedCount);
	}
}
