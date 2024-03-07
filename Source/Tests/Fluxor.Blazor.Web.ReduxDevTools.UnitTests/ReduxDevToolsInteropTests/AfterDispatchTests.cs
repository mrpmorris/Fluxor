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
	private Lazy<ReduxDevToolsMiddleware> Subject;

	[Fact]
	public async Task WhenUsingActionFiltering_AndAnyFilterReturnsFalse_ThenActionShouldNotBeLogged()
	{
		Options.AddActionFilter(_ => false);
		Options.AddActionFilter(_ => true);
		Subject.Value.AfterDispatch(this);
		await Task.Yield();

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
		Subject.Value.AfterDispatch(this);
		await Task.Yield();
		
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
		Subject.Value.AfterDispatch(this);
		await Task.Yield(); 
		
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
		Subject = new(() =>
		{

			var result = new ReduxDevToolsMiddleware(
				MockReduxDevToolsInterop.Object,
				Options);
			result.InitializeAsync(Mock.Of<IDispatcher>(), new Store(Mock.Of<IDispatcher>()));
			return result;
		});

	}
}

#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed