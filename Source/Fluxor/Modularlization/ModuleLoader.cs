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

		public void Load(IStore store, Assembly assemblyToScan) =>
			Load(store, new List<Assembly> { assemblyToScan });

		public void Load(IStore store, IEnumerable<Assembly> assembliesToScan)
		{
			if (assembliesToScan == null)
				throw new ArgumentNullException(nameof(assembliesToScan));
			if (!assembliesToScan.Any())
				throw new ArgumentException("At least one assembly is required", nameof(assembliesToScan));

			var options = new Options(null);
			options.ScanAssemblies(assembliesToScan.First(), assembliesToScan.Skip(1).ToArray());

			DependencyScanner.Scan(
				options,
				out DiscoveredReducerClass[] discoveredReducerClasses,
				out DiscoveredReducerMethod[] discoveredReducerMethods,
				out DiscoveredEffectClass[] discoveredEffectClasses,
				out DiscoveredEffectMethod[] discoveredEffectMethods,
				out DiscoveredFeatureClass[] discoveredFeatureClasses,
				out DiscoveredMiddleware[] discoveredMiddlewares);

			// TODO: Build and add to store
		}
	}
}
