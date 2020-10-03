using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.UnitTests.StoreTests.AddMiddlewareTests
{
	public class AddMiddlewareTests
	{
		[Fact]
		public async Task WhenStoreHasAlreadyBeenInitialized_ThenCallsAfterInitializeAllMiddlewares()
		{
			var subject = new Store();
			var signal = new ManualResetEvent(false);
			var mockMiddleware = new Mock<IMiddleware>();
			mockMiddleware
				.Setup(x => x.AfterInitializeAllMiddlewares())
				.Callback(() => signal.Set());

			await subject.InitializeAsync();
			subject.AddMiddleware(mockMiddleware.Object);

			// Wait no more than 1 second for AfterInitializeAllMiddlewares to be executed
			signal.WaitOne(TimeSpan.FromSeconds(1));

			mockMiddleware
				.Verify(x => x.AfterInitializeAllMiddlewares());
		}
	}
}
