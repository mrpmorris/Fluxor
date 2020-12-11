using Fluxor.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fluxor.Modularlization
{
	internal class ModuleLoader : IModuleLoader
	{
		private readonly IServiceProvider ServiceProvider;

		public ModuleLoader(IServiceProvider serviceProvider)
		{
			ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		public void Load(
			IStore store,
			IEnumerable<Assembly> assembliesToScan,
			IEnumerable<Type> middlewareTypes)
		{
			if (assembliesToScan == null)
				throw new ArgumentNullException(nameof(assembliesToScan));
			if (!assembliesToScan.Any())
				throw new ArgumentException("At least one assembly is required", nameof(assembliesToScan));

			middlewareTypes = middlewareTypes ?? Array.Empty<Type>();
			DependencyScanner.Scan(
				assembliesToScan,
				middlewareTypes,
				out DiscoveredFeatureClass[] discoveredFeatureClasses,
				out DiscoveredReducerClass[] discoveredReducerClasses,
				out DiscoveredReducerMethod[] discoveredReducerMethods,
				out DiscoveredEffectClass[] discoveredEffectClasses,
				out DiscoveredEffectMethod[] discoveredEffectMethods,
				out DiscoveredMiddleware[] discoveredMiddlewares);

			// TODO: Build and add to store
		}
	}
}
