using Fluxor.UnitTests.SupportFiles;
using System;
using Xunit;

namespace Fluxor.UnitTests.ActionSubscriberTests
{
	public class UnsubscribeFromAllActionsTests
	{
		private Store Subject = new Store();

		public UnsubscribeFromAllActionsTests()
		{
			Subject.InitializeAsync().Wait();
		}

		[Fact]
		public void WhenExecuted_ThenNoFutherSubscriptionsAreTriggeredForSubscriber()
		{
			Subject.SubscribeToAction<TestAction>(this, x => throw new InvalidOperationException("Subscriber should not be triggered"));
			Subject.UnsubscribeFromAllActions(this);
			Subject.Dispatch(new TestAction());
		}

		[Fact]
		public void WhenExecuted_ThenOnlyUnsubscribesTheSpecifiedSubscriber()
		{
			var subscriber1 = new object();
			var subscriber2 = new object();
			var dispatchedAction = new TestAction();
			TestAction actionReceivedBySubscriber1 = null;
			TestAction actionReceivedBySubscriber2 = null;

			Subject.SubscribeToAction<TestAction>(subscriber1, x => actionReceivedBySubscriber1 = x);
			Subject.SubscribeToAction<TestAction>(subscriber2, x => actionReceivedBySubscriber2 = x);
			Subject.UnsubscribeFromAllActions(subscriber1);
			Subject.Dispatch(dispatchedAction);

			Assert.Null(actionReceivedBySubscriber1);
			Assert.Same(dispatchedAction, actionReceivedBySubscriber2);
		}

		[Fact]
		public void WhenExecutedFromSubscriptionCallback_ThenDoesNotThrowAnError()
		{
			int executionCount = 0;
			var subscriber = new object();
			var dispatchedAction = new TestAction();

			Subject.SubscribeToAction<TestAction>(subscriber, x =>
			{
				executionCount++;
				Subject.UnsubscribeFromAllActions(subscriber);
			});

			Subject.Dispatch(dispatchedAction);
			Subject.Dispatch(dispatchedAction);

			Assert.Equal(1, executionCount);
		}
	}
}
