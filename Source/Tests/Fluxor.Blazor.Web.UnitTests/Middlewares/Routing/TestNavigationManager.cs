using Microsoft.AspNetCore.Components;

namespace Fluxor.Blazor.Web.UnitTests.Middlewares.Routing;

internal class TestNavigationManager : NavigationManager
{
	public TestNavigationManager(string baseUrl, string path)
	{
		Initialize(baseUrl, path);
	}

	protected override void NavigateToCore(string uri, NavigationOptions options)
	{
		Uri = uri;
		NotifyLocationChanged(isInterceptedLink: false);
	}
}
