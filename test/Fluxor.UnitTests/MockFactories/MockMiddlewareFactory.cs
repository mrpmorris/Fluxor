using Moq;

namespace Fluxor.UnitTests.MockFactories
{
	public static class MockMiddlewareFactory
	{
		public static Mock<IMiddleware> Create()
		{
			var mock = new Mock<IMiddleware>();
			mock
				.Setup(x => x.BeginInternalMiddlewareChange())
				.Returns(new DisposableCallback(() => { }));
			mock
				.Setup(x => x.MayDispatchAction(It.IsAny<object>()))
				.Returns(true);
			return mock;
		}
	}
}
