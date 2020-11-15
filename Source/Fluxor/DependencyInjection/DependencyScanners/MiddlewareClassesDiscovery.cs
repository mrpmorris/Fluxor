using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fluxor.DependencyInjection.DependencyScanners
{
	internal static class MiddlewareClassesDiscovery
	{
		internal static DiscoveredMiddleware[] FindMiddlewares(IEnumerable<Assembly> assembliesToScan)
		{
			return assembliesToScan
				.SelectMany(a => a.GetTypes().Where(t => t.GetInterfaces().Any(i => i == typeof(IMiddleware))))
				.Select(t => 
					new DiscoveredMiddleware(
						implementingType: t,
						autoLoaded: t.GetCustomAttribute(typeof(AutoLoadMiddlewareAttribute)) != null))
				.ToArray();
		}
	}
}
