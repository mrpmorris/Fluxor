using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.UnitTests.StoreTests.InitializeAsyncTests
{
	public class InitializeAsyncTests
	{
		[Fact]
		public async Task WhenCalled_ThenCallsInitializeAsyncOnRegisteredMiddlewares()
		{
			var subject = new Store();
			await subject.InitializeAsync();
			var mockMiddleware = new Mock<IMiddleware>();
			subject.AddMiddleware(mockMiddleware.Object);

			mockMiddleware
				.Verify(x => x.InitializeAsync(subject));
		}

		[Fact]
		public async Task WhenCalled_ThenCallsAfterInitializeAllMiddlewaresOnRegisteredMiddlewares()
		{
			var subject = new Store();
			var mockMiddleware = new Mock<IMiddleware>();
			subject.AddMiddleware(mockMiddleware.Object);

			await subject.InitializeAsync();

			mockMiddleware
				.Verify(x => x.AfterInitializeAllMiddlewares());
		}

		[Fact]
		public async Task WhenStoreIsInitialized_ThenCallsInitializeAsyncOnAllRegisteredMiddlewares()
		{
			var subject = new Store();
			await subject.InitializeAsync();

			var mockMiddleware = new Mock<IMiddleware>();
			subject.AddMiddleware(mockMiddleware.Object);

			mockMiddleware
				.Verify(x => x.InitializeAsync(subject));
		}
	}
}