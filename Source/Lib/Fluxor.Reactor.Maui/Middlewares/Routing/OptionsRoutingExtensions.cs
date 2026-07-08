using Fluxor.Reactor.Maui.Middlewares.Routing;
using Fluxor.DependencyInjection;

namespace Fluxor;

public static class OptionsRoutingExtensions
{
	public static FluxorOptions UseRouting(this FluxorOptions options)
	{
		options.AddMiddleware<RoutingMiddleware>();
		return options;
	}
}
