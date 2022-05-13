using Fluxor.UnitTests.ActionSubscriberTests.GetActionUnsubscriberAsIDisposableTests.SupportFiles;
using System;
using Xunit;

namespace Fluxor.UnitTests.ActionSubscriberTests.GetActionUnsubscriberAsIDisposableTests
{
	public class GetActionUnsubscriberAsIDisposableTests
	{
		private Dispatcher Dispatcher;
		private Store Subject;

		public GetActionUnsubscriberAsIDisposableTests()
		{
			Dispatcher = new Dispatcher();
			Subject = new Store(Dispatcher);
			Subject.InitializeAsync().Wait();
		}

		[Fact]
		public void WhenExecuted_ThenNoFutherSubscriptionsAreTriggeredForSubscriber()
		{
			Subject.SubscribeToAction<TestAction>(this, x => throw new InvalidOperationException("Subscriber should not be triggered"));
			Subject.GetActionUnsubscriberAsIDisposable(this).Dispose();
			Dispatcher.Dispatch(new TestAction());
		}
	}
}
