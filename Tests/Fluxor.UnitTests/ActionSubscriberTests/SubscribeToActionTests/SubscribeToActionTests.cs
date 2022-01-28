using Fluxor.UnitTests.ActionSubscriberTests.SubscribeToActionTests.SupportFiles;
using System;
using Xunit;

namespace Fluxor.UnitTests.ActionSubscriberTests.SubscribeToActionTests
{
	public class SubscribeToActionTests
	{
		private Dispatcher Dispatcher;
		private Store Subject;

		public SubscribeToActionTests()
		{
			Dispatcher = new Dispatcher();
			Subject = new Store(Dispatcher);
			Subject.InitializeAsync().Wait();
		}

		[Fact]
		public void WhenSubscriberIsNull_ThenThrowArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>("subscriber", () => Subject.SubscribeToAction<object>(subscriber: null, callback: null));
		}

		[Fact]
		public void WhenCallbackIsNull_ThenThrowArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>("callback", () => Subject.SubscribeToAction<object>(subscriber: this, callback: null));
		}

		[Fact]
		public void WhenActionIsDispatched_ThenTriggersSubscription()
		{
			var dispatchedAction = new TestAction();
			TestAction receivedAction = null;

			Subject.SubscribeToAction<TestAction>(this, x => receivedAction = x);
			Dispatcher.Dispatch(dispatchedAction);

			Assert.Same(dispatchedAction, receivedAction);
		}

		[Fact]
		public void WhenDescendantActionIsDispatched_ThenTriggersSubscription()
		{
			var dispatchedAction = new TestDescendantAction();
			TestAction receivedAction = null;

			Subject.SubscribeToAction<TestAction>(this, x => receivedAction = x);
			Dispatcher.Dispatch(dispatchedAction);

			Assert.Same(dispatchedAction, receivedAction);
		}

		[Fact]
		public void WhenSubscribingToTheSameActionTypeMultipleTimes_ThenTriggersEachSubscription()
		{
			var dispatchedAction = new TestAction();
			TestAction receivedAction1 = null;
			TestAction receivedAction2 = null;

			Subject.SubscribeToAction<TestAction>(this, x => receivedAction1 = x);
			Subject.SubscribeToAction<TestAction>(this, x => receivedAction2 = x);
			Dispatcher.Dispatch(dispatchedAction);

			Assert.Same(dispatchedAction, receivedAction1);
			Assert.Same(dispatchedAction, receivedAction2);
		}

		[Fact]
		public void WhenAncestorActionIsDispatched_ThenDoesNotTriggerSubscriptionsForDescendantActionTypes()
		{
			var dispatchedAncestorAction = new TestAction();
			TestAction receivedAncestorAction = null;
			TestDescendantAction receivedDescendantAction = null;

			Subject.SubscribeToAction<TestAction>(this, x => receivedAncestorAction = x);
			Subject.SubscribeToAction<TestDescendantAction>(this, x => receivedDescendantAction = x);
			Dispatcher.Dispatch(dispatchedAncestorAction);

			Assert.Same(dispatchedAncestorAction, receivedAncestorAction);
			Assert.Null(receivedDescendantAction);
		}

		[Fact]
		public void WhenDescenantActionIsDispatched_ThenTriggersSubscriptionsForAncestorActionTypes()
		{
			var dispatchedDescendantAction = new TestDescendantAction();
			TestAction receivedAncestorAction = null;
			TestDescendantAction receivedDescendantAction = null;

			Subject.SubscribeToAction<TestAction>(this, x => receivedAncestorAction = x);
			Subject.SubscribeToAction<TestDescendantAction>(this, x => receivedDescendantAction = x);
			Dispatcher.Dispatch(dispatchedDescendantAction);

			Assert.Same(dispatchedDescendantAction, receivedAncestorAction);
			Assert.Same(dispatchedDescendantAction, receivedDescendantAction);
		}

		[Fact]
		public void WhenActionIsDispatched_ThenNotifiesMultipleSubscribers()
		{
			var subscriber1 = new object();
			var subscriber2 = new object();
			var dispatchedAction = new TestAction();
			TestAction actionReceivedBySubscriber1 = null;
			TestAction actionReceivedBySubscriber2 = null;

			Subject.SubscribeToAction<TestAction>(subscriber1, x => actionReceivedBySubscriber1 = x);
			Subject.SubscribeToAction<TestAction>(subscriber2, x => actionReceivedBySubscriber2 = x);
			Dispatcher.Dispatch(dispatchedAction);

			Assert.Same(dispatchedAction, actionReceivedBySubscriber1);
			Assert.Same(dispatchedAction, actionReceivedBySubscriber2);
		}

		[Fact]
		public void WhenActionIsDispatched_ThenNotifiesOnlySubscribedSubscribers()
		{
			var subscriber1 = new object();
			var subscriber2 = new object();
			var dispatchedAction = new TestAction();
			TestAction actionReceivedBySubscriber1 = null;
			TestAction actionReceivedBySubscriber2 = null;

			Subject.SubscribeToAction<TestAction>(subscriber1, x => actionReceivedBySubscriber1 = x);
			Dispatcher.Dispatch(dispatchedAction);

			Assert.Same(dispatchedAction, actionReceivedBySubscriber1);
			Assert.Null(actionReceivedBySubscriber2);
		}

		[Fact]
		public void WhenGenericActionIsDispatched_ThenNotifiesSubscriber()
        {
			TestReadonlyRecordStructAction? receivedAction = null;

			Subject.SubscribeToAction<TestReadonlyRecordStructAction>(this, x => receivedAction = x);
			Dispatcher.Dispatch<TestReadonlyRecordStructAction>();

			Assert.Equal(default(TestReadonlyRecordStructAction), receivedAction);
		}
	}
}
