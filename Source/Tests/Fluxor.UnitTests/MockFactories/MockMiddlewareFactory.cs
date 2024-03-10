using Moq;
using System;

namespace Fluxor.UnitTests.MockFactories;

public static class MockMiddlewareFactory
{
	public static Mock<IMiddleware> Create()
	{
		var mock = new Mock<IMiddleware>();
		mock
			.Setup(x => x.BeginInternalMiddlewareChange())
			.Returns(Mock.Of<IDisposable>());
		mock
			.Setup(x => x.MayDispatchAction(It.IsAny<object>()))
			.Returns(true);
		return mock;
	}
}
