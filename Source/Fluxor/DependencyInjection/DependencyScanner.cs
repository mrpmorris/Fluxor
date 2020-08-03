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

			IEnumerable<Type> allCandidateTypes =
				assembliesToScan
				.SelectMany(x => GetLoadedTypes(x.Assembly))
				.Union(scanIncludeList.SelectMany(x => GetLoadedTypes(x.Assembly)))
				.Distinct()
				.ToArray();

			IEnumerable<Type> allNonAbstractCandidateTypes =
				allCandidateTypes
				.Where(t => !t.IsAbstract)
				.ToArray();

			IEnumerable<Assembly> allCandidateAssemblies =
				assembliesToScan
				.Select(x => x.Assembly)
				.Union(scanIncludeList.Select(x => x.Assembly))
				.Distinct()
				.ToArray();

			IEnumerable<AssemblyScanSettings> scanExcludeList =
				MiddlewareClassesDiscovery.FindMiddlewareLocations(allCandidateAssemblies);

			allCandidateTypes = AssemblyScanSettings.FilterClasses(
				types: allCandidateTypes,
				scanExcludeList: scanExcludeList,
				scanIncludeList: scanIncludeList);

			IEnumerable<MethodInfo> allCandidateMethods = AssemblyScanSettings.FilterMethods(allCandidateTypes);

			IEnumerable<DiscoveredReducerClass> discoveredReducerClasses =
				ReducerClassessDiscovery.DiscoverReducerClasses(serviceCollection, allNonAbstractCandidateTypes);

			IEnumerable<DiscoveredReducerMethod> discoveredReducerMethods =
				ReducerMethodsDiscovery.DiscoverReducerMethods(serviceCollection, allCandidateMethods);

			IEnumerable<DiscoveredEffectClass> discoveredEffectClasses =
				EffectClassessDiscovery.DiscoverEffectClasses(serviceCollection, allNonAbstractCandidateTypes);

			IEnumerable<DiscoveredEffectMethod> discoveredEffectMethods =
				EffectMethodsDiscovery.DiscoverEffectMethods(serviceCollection, allCandidateMethods);

			IEnumerable<DiscoveredFeatureClass> discoveredFeatureClasses =
				FeatureClassesDiscovery.DiscoverFeatureClasses(
					serviceCollection,
					allNonAbstractCandidateTypes,
					discoveredReducerClasses,
					discoveredReducerMethods);

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
			// Register IDispatcher as an alias to IStore
			serviceCollection.AddScoped<IDispatcher>(serviceProvider => serviceProvider.GetService<IStore>());

			// Register a custom factory for building IStore that will inject all effects
			serviceCollection.AddScoped(typeof(IStore), serviceProvider =>
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
		public static IEnumerable<Type> GetLoadedTypes(Assembly assembly)
		{
			try
		    	{
				return assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException ex)
			{
				return ex.Types.Where(x => x != null);
			}
		}
	}
}
