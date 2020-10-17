using Fluxor.UnitTests.ActionSubscriberTests.GetActionUnsubscriberAsIDisposableTests.SupportFiles;
using System;
using Xunit;

namespace Fluxor.UnitTests.ActionSubscriberTests.GetActionUnsubscriberAsIDisposableTests
{
	public class GetActionUnsubscriberAsIDisposableTests
	{
		private Store Subject = new Store();

		public GetActionUnsubscriberAsIDisposableTests()
		{
			Subject.InitializeAsync().Wait();
		}

		[Fact]
		public void WhenExecuted_ThenNoFutherSubscriptionsAreTriggeredForSubscriber()
		{
			Subject.SubscribeToAction<TestAction>(this, x => throw new InvalidOperationException("Subscriber should not be triggered"));
			Subject.GetActionUnsubscriberAsIDisposable(this).Dispose();
			Subject.Dispatch(new TestAction());
		}
	}
}
