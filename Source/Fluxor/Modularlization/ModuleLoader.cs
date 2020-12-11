using Fluxor.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
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

			FixUpStore(
				store,
				discoveredFeatureClasses,
				discoveredReducerClasses,
				discoveredReducerMethods,
				discoveredEffectClasses,
				discoveredEffectMethods,
				discoveredMiddlewares);
		}

		private void FixUpStore(
			IStore store,
			DiscoveredFeatureClass[] discoveredFeatureClasses,
			DiscoveredReducerClass[] discoveredReducerClasses,
			DiscoveredReducerMethod[] discoveredReducerMethods,
			DiscoveredEffectClass[] discoveredEffectClasses,
			DiscoveredEffectMethod[] discoveredEffectMethods,
			DiscoveredMiddleware[] discoveredMiddlewares)
		{
			var objectBuilder = new ObjectBuilder(ServiceProvider);
			foreach (DiscoveredFeatureClass discoveredFeatureClass in discoveredFeatureClasses)
			{
				var feature = (IFeature)objectBuilder.Build(discoveredFeatureClass.ImplementingType);
				store.AddFeature(feature);
				// TODO: PeteM - Fixup featue
			}

			foreach (DiscoveredEffectClass discoveredEffectClass in discoveredEffectClasses)
			{
				var effect = (IEffect)objectBuilder.Build(discoveredEffectClass.ImplementingType);
				store.AddEffect(effect);
			}

			foreach (DiscoveredEffectMethod discoveredEffectMethod in discoveredEffectMethods)
			{
				IEffect effect = EffectWrapperFactory.Create(objectBuilder, discoveredEffectMethod);
				store.AddEffect(effect);
			}

			Type[] autoLoadedMiddlewareTypes = discoveredMiddlewares
				.Where(x => x.AutoLoaded)
				.Select(x => x.ImplementingType)
				.Distinct()
				.ToArray();

			Type[] middlewareTypes = discoveredMiddlewares
				.Select(x => x.ImplementingType)
				.Union(autoLoadedMiddlewareTypes)
				.Distinct()
				.ToArray();

			foreach (Type middlewareType in middlewareTypes)
			{
				var middleware = (IMiddleware)objectBuilder.Build(middlewareType);
				store.AddMiddleware(middleware);
			}
		}
	}
}
