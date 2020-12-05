using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.UnitTests.StoreTests.InitializeAsyncTests
{
	public class InitializeAsyncTests
	{
		private IDispatcher Dispatcher;
		private IStore Subject;

		[Fact]
		public async Task WhenCalled_ThenCallsInitializeAsyncOnRegisteredMiddlewares()
		{
			await Subject.InitializeAsync().ConfigureAwait(false);
			var mockMiddleware = new Mock<IMiddleware>();
			Subject.AddMiddleware(mockMiddleware.Object);

			mockMiddleware
				.Verify(x => x.InitializeAsync(Dispatcher, Subject));
		}

		[Fact]
		public async Task WhenCalled_ThenCallsAfterInitializeAllMiddlewaresOnRegisteredMiddlewares()
		{
			var mockMiddleware = new Mock<IMiddleware>();
			Subject.AddMiddleware(mockMiddleware.Object);

			await Subject.InitializeAsync().ConfigureAwait(false);

			mockMiddleware
				.Verify(x => x.AfterInitializeAllMiddlewares());
		}

		[Fact]
		public async Task WhenStoreIsInitialized_ThenCallsInitializeAsyncOnAllRegisteredMiddlewares()
		{
			await Subject.InitializeAsync().ConfigureAwait(false);

			var mockMiddleware = new Mock<IMiddleware>();
			Subject.AddMiddleware(mockMiddleware.Object);

			mockMiddleware
				.Verify(x => x.InitializeAsync(Dispatcher, Subject));
		}

		public InitializeAsyncTests()
		{
			Dispatcher = new Dispatcher();
			Subject = new Store(Dispatcher);
		}
	}
}