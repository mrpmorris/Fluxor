using Fluxor.Blazor.Web.Middlewares.Routing;
using Fluxor.DependencyInjection;

namespace Fluxor
{
	public static class OptionsRoutingExtensions
	{
		public static FluxorOptions UseRouting(this FluxorOptions options)
		{
			options.AddMiddleware<RoutingMiddleware>();
			return options;
		}
	}
}
