using Fluxor.Blazor.Web.Middlewares.Routing;
using Fluxor.DependencyInjection;

namespace Fluxor
{
	public static class OptionsRoutingExtensions
	{
		public static Options UseRouting(this Options options)
		{
			options.AddMiddleware<RoutingMiddleware>();
			return options;
		}
	}
}
