using Moq;
using Xunit;

namespace Fluxor.UnitTests.StoreTests.BeginInternalMiddlewareChangeTests
{
	public class BeginInternalMiddlewareChangeTests
	{
		[Fact]
		public void WhenCalled_ThenExecutesOnAllRegisteredMiddlewares()
		{
			int disposeCount = 0;
			var mockMiddleware = new Mock<IMiddleware>();
			mockMiddleware
				.Setup(x => x.BeginInternalMiddlewareChange())
				.Returns(new DisposableCallback(
					$"{nameof(BeginInternalMiddlewareChangeTests)}.{nameof(WhenCalled_ThenExecutesOnAllRegisteredMiddlewares)}",
					() => disposeCount++));

			var subject = new Store();
			subject.AddMiddleware(mockMiddleware.Object);

			var disposable1 = subject.BeginInternalMiddlewareChange();
			var disposable2 = subject.BeginInternalMiddlewareChange();

			disposable1.Dispose();
			Assert.Equal(0, disposeCount);

			disposable2.Dispose();
			Assert.Equal(1, disposeCount);
		}
	}
}

