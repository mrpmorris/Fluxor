using Fluxor.UnitTests.ActionSubscriberTests.GetActionUnsubscriberAsIDisposableTests.SupportFiles;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.UnitTests.ActionSubscriberTests.GetActionUnsubscriberAsIDisposableTests;

public class GetActionUnsubscriberAsIDisposableTests : IAsyncLifetime
{
	private Dispatcher Dispatcher;
	private Store Subject;

	[Fact]
	public void WhenExecuted_ThenNoFutherSubscriptionsAreTriggeredForSubscriber()
	{
		Subject.SubscribeToAction<TestAction>(this, x => throw new InvalidOperationException("Subscriber should not be triggered"));
		Subject.GetActionUnsubscriberAsIDisposable(this).Dispose();
		Dispatcher.Dispatch(new TestAction());
	}

	public GetActionUnsubscriberAsIDisposableTests()
	{
		Dispatcher = new Dispatcher();
		Subject = new Store(Dispatcher);
	}

	async Task IAsyncLifetime.InitializeAsync() =>
		await Subject.InitializeAsync();

	Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;
}
