#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
using Fluxor.Blazor.Web.ReduxDevTools.Internal;
using Fluxor.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Fluxor.Blazor.Web.ReduxDevTools.UnitTests.ReduxDevToolsInteropTests;

public class AfterDispatchTests
{
	private readonly Mock<IReduxDevToolsInterop> MockReduxDevToolsInterop;
	private ReduxDevToolsMiddlewareOptions Options;
	private readonly Mock<IJsonSerialization> MockJsonSerialization;
	private Lazy<ReduxDevToolsMiddleware> Subject;

	[Fact]
	public void WhenUsingActionFiltering_AndFilterReturnsFalse_ThenActionShouldNotBeLogged()
	{
		Options.AddActionFilter(_ => false);
		Options.AddActionFilter(_ => true);
		Subject.Value.AfterDispatch(this);
		MockReduxDevToolsInterop.Verify(
			x => x.DispatchAsync(
				this,
				It.IsAny<IDictionary<string, object>>(),
				It.IsAny<string>()),
			Times.Never,
			"Should have logged action");
	}

	[Fact]
	public void WhenUsingActionFiltering_AndFilterReturnsTrue_ThenActionShouldBeLogged()
	{
		Options.AddActionFilter(_ => true);
		Subject.Value.AfterDispatch(this);
		MockReduxDevToolsInterop.Verify(
			x => x.DispatchAsync(
				this,
				It.IsAny<IDictionary<string, object>>(),
				It.IsAny<string>()),
			Times.Once,
			"Should have logged action");
	}

	[Fact]
	public void WhenNotUsingActionFiltering_ThenActionShouldBeLogged()
	{
		Subject.Value.AfterDispatch(this);
		MockReduxDevToolsInterop.Verify(
			x => x.DispatchAsync(
				this,
				It.IsAny<IDictionary<string, object>>(),
				It.IsAny<string>()),
			Times.Once,
			"Should have logged action");
	}

	public AfterDispatchTests()
	{
		MockReduxDevToolsInterop = new Mock<IReduxDevToolsInterop>();
		Options = new ReduxDevToolsMiddlewareOptions(new FluxorOptions(new ServiceCollection()));
		MockJsonSerialization = new Mock<IJsonSerialization>();
		Subject = new(() =>
		{

			var result = new ReduxDevToolsMiddleware(
				MockReduxDevToolsInterop.Object,
				Options,
				MockJsonSerialization.Object);
			result.InitializeAsync(Mock.Of<IDispatcher>(), new Store(Mock.Of<IDispatcher>()));
			return result;
		});

	}
}

#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed