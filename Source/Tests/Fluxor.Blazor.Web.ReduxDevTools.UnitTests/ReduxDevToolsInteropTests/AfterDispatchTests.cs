using Fluxor.Blazor.Web.ReduxDevTools.Internal;
using Fluxor.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Fluxor.Blazor.Web.ReduxDevTools.UnitTests.ReduxDevToolsInteropTests;

public class AfterDispatchTests : IAsyncLifetime
{
	private readonly Mock<IReduxDevToolsInterop> MockReduxDevToolsInterop;
	private ReduxDevToolsMiddlewareOptions Options;
	private ReduxDevToolsMiddleware Subject;

	[Fact]
	public async Task WhenUsingActionFiltering_AndAnyFilterReturnsFalse_ThenActionShouldNotBeLogged()
	{
		Options.AddActionFilter(_ => false);
		Options.AddActionFilter(_ => true);
		Subject.AfterDispatch(this);
		await Task.Delay(50);

		MockReduxDevToolsInterop.Verify(
			x => x.DispatchAsync(
				this,
				It.IsAny<IDictionary<string, object>>(),
				It.IsAny<string>()),
			Times.Never,
			"Should have logged action");
	}

	[Fact]
	public async Task WhenUsingActionFiltering_AndFilterReturnsTrue_ThenActionShouldBeLogged()
	{
		Options.AddActionFilter(_ => true);
		Subject.AfterDispatch(this);
		await Task.Delay(50);
		
		MockReduxDevToolsInterop.Verify(
			x => x.DispatchAsync(
				this,
				It.IsAny<IDictionary<string, object>>(),
				It.IsAny<string>()),
			Times.Once,
			"Should have logged action");
	}

	[Fact]
	public async Task WhenNotUsingActionFiltering_ThenActionShouldBeLogged()
	{
		Subject.AfterDispatch(this);
		await Task.Delay(50);
		
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
		Subject =
			new ReduxDevToolsMiddleware(
				MockReduxDevToolsInterop.Object,
				Options);
	}

	async Task IAsyncLifetime.InitializeAsync() =>
		await Subject.InitializeAsync(Mock.Of<IDispatcher>(), new Store(Mock.Of<IDispatcher>()));

	Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

}
