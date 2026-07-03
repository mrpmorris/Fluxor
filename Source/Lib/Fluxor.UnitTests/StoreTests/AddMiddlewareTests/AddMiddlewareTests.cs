using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.UnitTests.StoreTests.AddMiddlewareTests;

public class AddMiddlewareTests
{
	[Fact]
	public async Task WhenStoreHasAlreadyBeenInitialized_ThenCallsAfterInitializeAllMiddlewares()
	{
		var dispatcher = new Dispatcher();
		var subject = new Store(dispatcher);
		var signal = new ManualResetEvent(false);
		var mockMiddleware = new Mock<IMiddleware>();
		mockMiddleware
			.Setup(x => x.AfterInitializeAllMiddlewares())
			.Callback(() => signal.Set());

		await subject.InitializeAsync();
		subject.AddMiddleware(mockMiddleware.Object);

		// AfterInitializeAllMiddlewares is executed via a thread-pool continuation, which can be
		// slow to schedule on a busy CI agent. WaitOne returns as soon as the signal is set, so a
		// generous timeout only slows down the failure case.
		bool afterInitializeCalled = signal.WaitOne(TimeSpan.FromSeconds(30));
		Assert.True(afterInitializeCalled, "Timed out waiting for AfterInitializeAllMiddlewares");

		mockMiddleware
			.Verify(x => x.AfterInitializeAllMiddlewares());
	}
}
