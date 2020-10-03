using Fluxor.DependencyInjection.DependencyScanners;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fluxor.DependencyInjection
{
	internal static class DependencyScanner
	{
		internal static void Scan(
			this IServiceCollection serviceCollection,
			IEnumerable<AssemblyScanSettings> assembliesToScan,
			IEnumerable<AssemblyScanSettings> scanIncludeList)
		{
			if (assembliesToScan == null || assembliesToScan.Count() == 0)
				throw new ArgumentNullException(nameof(assembliesToScan));
			scanIncludeList = scanIncludeList ?? new List<AssemblyScanSettings>();

			Assembly[] allCandidateAssemblies =
				assembliesToScan
					.Select(x => x.Assembly)
					.Union(scanIncludeList.Select(x => x.Assembly))
					.Distinct()
					.ToArray();

			AssemblyScanSettings[] scanExcludeList =
				MiddlewareClassesDiscovery.FindMiddlewareLocations(allCandidateAssemblies);

			Type[] allCandidateTypes = AssemblyScanSettings.FilterClasses(
				scanExcludeList: scanExcludeList,
				scanIncludeList: scanIncludeList,
				types:
					allCandidateAssemblies
						.SelectMany(x => x.GetTypes())
						.Union(scanIncludeList.SelectMany(x => x.Assembly.GetTypes()))
						.Distinct()
						.Where(t => !t.IsAbstract)
						.ToArray());

			MethodInfo[] allCandidateMethods = AssemblyScanSettings.FilterMethods(allCandidateTypes);

			DiscoveredReducerClass[] discoveredReducerClasses =
				ReducerClassessDiscovery.DiscoverReducerClasses(serviceCollection, allCandidateTypes);

			DiscoveredReducerMethod[] discoveredReducerMethods =
				ReducerMethodsDiscovery.DiscoverReducerMethods(serviceCollection, allCandidateMethods);

			DiscoveredEffectClass[] discoveredEffectClasses =
				EffectClassessDiscovery.DiscoverEffectClasses(serviceCollection, allCandidateTypes);

			DiscoveredEffectMethod[] discoveredEffectMethods =
				EffectMethodsDiscovery.DiscoverEffectMethods(serviceCollection, allCandidateMethods);

			DiscoveredFeatureClass[] discoveredFeatureClasses =
				FeatureClassesDiscovery.DiscoverFeatureClasses(
					serviceCollection,
					allCandidateTypes,
					discoveredReducerClasses,
					discoveredReducerMethods); ; ;

			RegisterStore(
				serviceCollection,
				discoveredFeatureClasses,
				discoveredEffectClasses,
				discoveredEffectMethods);
		}

		private static void RegisterStore(IServiceCollection serviceCollection,
			IEnumerable<DiscoveredFeatureClass> discoveredFeatureClasses,
			IEnumerable<DiscoveredEffectClass> discoveredEffectClasses,
			IEnumerable<DiscoveredEffectMethod> discoveredEffectMethods)
		{
			// Register IDispatcher as an alias to Store
			serviceCollection.AddScoped<IDispatcher>(serviceProvider => serviceProvider.GetService<Store>());
			// Register IActionSubscriber as an alias to Store
			serviceCollection.AddScoped<IActionSubscriber>(serviceProvider => serviceProvider.GetService<Store>());
			// Register IStore as an alias to Store
			serviceCollection.AddScoped<IStore>(serviceProvider => serviceProvider.GetService<Store>());

			// Register a custom factory for building IStore that will inject all effects
			serviceCollection.AddScoped(typeof(Store), serviceProvider =>
			{
				var store = new Store();
				foreach (DiscoveredFeatureClass discoveredFeatureClass in discoveredFeatureClasses)
				{
					var feature = (IFeature)serviceProvider.GetService(discoveredFeatureClass.FeatureInterfaceGenericType);
					store.AddFeature(feature);
				}

				foreach (DiscoveredEffectClass discoveredEffectClass in discoveredEffectClasses)
				{
					var effect = (IEffect)serviceProvider.GetService(discoveredEffectClass.ImplementingType);
					store.AddEffect(effect);
				}

				foreach (DiscoveredEffectMethod discoveredEffectMethod in discoveredEffectMethods)
				{
					IEffect effect = EffectWrapperFactory.Create(serviceProvider, discoveredEffectMethod);
					store.AddEffect(effect);
				}

				foreach (Type middlewareType in Options.MiddlewareTypes)
				{
					var middleware = (IMiddleware)serviceProvider.GetService(middlewareType);
					store.AddMiddleware(middleware);
				}

				return store;
			});
		}
	}
}
