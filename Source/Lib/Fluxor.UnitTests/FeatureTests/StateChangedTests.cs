using Fluxor.UnitTests.FeatureTests.SupportFiles;
using System;
using Xunit;

namespace Fluxor.UnitTests.FeatureTests;

public class StateChangedTests
{
	[Fact]
	public void WhenAReducerChangesTheState_ThenStateChangedIsRaised()
	{
		var subject = new SomeFeature();
		subject.AddReducer(new IncrementReducer());
		object sender = null;
		subject.StateChanged += (s, _) => sender = s;

		subject.ReceiveDispatchNotificationFromStore(new IncrementAction());

		Assert.Same(subject, sender);
		Assert.Equal(2, subject.State.SomeValue);
	}

	[Fact]
	public void WhenAReducerReturnsTheSameState_ThenStateChangedIsNotRaised()
	{
		var subject = new SomeFeature();
		subject.AddReducer(new SameStateReducer());
		int raised = 0;
		subject.StateChanged += (_, _) => raised++;

		subject.ReceiveDispatchNotificationFromStore(new IncrementAction());

		Assert.Equal(0, raised);
	}

	[Fact]
	public void WhenStateChangedIsUnsubscribed_ThenItIsNoLongerRaised()
	{
		var subject = new SomeFeature();
		subject.AddReducer(new IncrementReducer());
		int raised = 0;
		EventHandler handler = (_, _) => raised++;
		subject.StateChanged += handler;

		subject.ReceiveDispatchNotificationFromStore(new IncrementAction());
		subject.StateChanged -= handler;
		subject.ReceiveDispatchNotificationFromStore(new IncrementAction());

		Assert.Equal(1, raised);
	}

	[Fact]
	public void WhenStateIsReadRepeatedly_ThenTheInitialStateIsOnlyCreatedOnce()
	{
		var subject = new CountingFeature();

		TestState first = subject.State;
		TestState second = subject.State;

		Assert.Same(first, second);
		Assert.Equal(1, subject.GetInitialStateCount);
	}

	[Fact]
	public void WhenCreated_ThenDebuggerBrowsableDefaultsToTrue()
	{
		Assert.True(new SomeFeature().DebuggerBrowsable);
		Assert.True(new FeatureStateAttribute().DebuggerBrowsable);
	}
}
