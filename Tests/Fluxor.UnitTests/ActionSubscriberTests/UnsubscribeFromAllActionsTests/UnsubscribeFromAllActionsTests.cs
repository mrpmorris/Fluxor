using Fluxor.UnitTests.ActionSubscriberTests.UnsubscribeFromAllActionsTests.SupportFiles;
using System;
using Xunit;

namespace Fluxor.UnitTests.ActionSubscriberTests.UnsubscribeFromAllActionsTests
{
	public class UnsubscribeFromAllActionsTests
	{
		private Dispatcher Dispatcher;
		private Store Subject;

		public UnsubscribeFromAllActionsTests()
		{
			Dispatcher = new Dispatcher();
			Subject = new Store(Dispatcher);
			Subject.InitializeAsync().Wait();
		}

		[Fact]
		public void WhenExecuted_ThenNoFutherSubscriptionsAreTriggeredForSubscriber()
		{
			Subject.SubscribeToAction<TestAction>(this, x => throw new InvalidOperationException("Subscriber should not be triggered"));
			Subject.UnsubscribeFromAllActions(this);
			Dispatcher.Dispatch(new TestAction());
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
			Dispatcher.Dispatch(dispatchedAction);

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

			Dispatcher.Dispatch(dispatchedAction);
			Dispatcher.Dispatch(dispatchedAction);

			Assert.Equal(1, executionCount);
		}
	}
}
