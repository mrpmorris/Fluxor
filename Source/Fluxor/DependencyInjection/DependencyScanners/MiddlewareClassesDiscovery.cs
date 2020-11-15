using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fluxor.DependencyInjection.DependencyScanners
{
	internal static class MiddlewareClassesDiscovery
	{
		internal static DiscoveredMiddleware[] FindMiddlewares(
			IServiceCollection services,
			IEnumerable<Assembly> assembliesToScan,
			IEnumerable<Type> manuallyIncludedMiddlewares)
		{
			var manuallyIncludedMiddlewaresLookup = new HashSet<Type>(manuallyIncludedMiddlewares);
			DiscoveredMiddleware[] discoveredMiddlewares = assembliesToScan
				.SelectMany(a => a.GetTypes().Where(t => t.GetInterfaces().Any(i => i == typeof(IMiddleware))))
				.Select(t =>
					new DiscoveredMiddleware(
						implementingType: t,
						autoLoaded: 
							manuallyIncludedMiddlewares.Contains(t)
							|| t.GetCustomAttribute(typeof(AutomaticallyIncludeMiddlewareAttribute)) != null))
				.ToArray();

			foreach (DiscoveredMiddleware discoveredMiddleware in discoveredMiddlewares)
				if (discoveredMiddleware.AutoLoaded)
					services.AddScoped(discoveredMiddleware.ImplementingType);
			return discoveredMiddlewares;
		}
	}
}
