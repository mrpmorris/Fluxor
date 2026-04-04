using Bunit;
using Fluxor.Blazor.Web.Middlewares.Routing;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.Blazor.Web.UnitTests.Middlewares.Routing.GoActionTests;

public class RelativeUrlTests
{
	[Fact]
	public async Task NavigateToRelativeUrl_WhenRelativeUrlProvided()
	{
		const string expected = "page2";
		
		var ctx = new TestContext();
		var nav = ctx.Services.GetRequiredService<NavigationManager>();
		nav.NavigateTo("page1");
		
		var goAction = new GoAction(expected);
		var effects = new Effects(nav);
		
		await effects.HandleGoActionAsync(goAction, null);
		
		Assert.EndsWith(expected, nav.Uri);
	}
}