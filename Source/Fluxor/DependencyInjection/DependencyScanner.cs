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
			IEnumerable<Assembly> assembliesToScan,
			IEnumerable<Type> middlewareTypes,
			out DiscoveredFeatureClass[] discoveredFeatureClasses,
			out DiscoveredReducerClass[] discoveredReducerClasses,
			out DiscoveredReducerMethod[] discoveredReducerMethods,
			out DiscoveredEffectClass[] discoveredEffectClasses,
			out DiscoveredEffectMethod[] discoveredEffectMethods,
			out DiscoveredMiddleware[] discoveredMiddlewares)
		{
			AssemblyScanSettings[] assemblyScanSettings = assembliesToScan
				.Select(x => new AssemblyScanSettings(x))
				.ToArray();

			AssemblyScanSettings[] scanIncludeList = middlewareTypes
				.Select(t => new AssemblyScanSettings(t.Assembly, t.Namespace))
				.Distinct()
				.ToArray();

			discoveredMiddlewares =
				MiddlewareClassesDiscovery.FindMiddlewares(
					assembliesToScan: assemblyScanSettings.Select(x => x.Assembly),
					manuallyIncludedMiddlewares: middlewareTypes);

			GetCandidateTypes(
				assembliesToScan: assemblyScanSettings,
				scanIncludeList: scanIncludeList,
				discoveredMiddlewares: discoveredMiddlewares,
				allCandidateTypes: out Type[] allCandidateTypes,
				allNonAbstractCandidateTypes: out Type[] allNonAbstractCandidateTypes);

			// Classes must not be abstract
			discoveredReducerClasses =
				ReducerClassessDiscovery.DiscoverReducerClasses(allNonAbstractCandidateTypes);

			discoveredEffectClasses =
				EffectClassessDiscovery.DiscoverEffectClasses(allNonAbstractCandidateTypes);

			// Method reducer/effects may belong to abstract classes
			TypeAndMethodInfo[] allCandidateMethods = AssemblyScanSettings.FilterMethods(allCandidateTypes);

			discoveredReducerMethods =
				ReducerMethodsDiscovery.DiscoverReducerMethods(allCandidateMethods);

			discoveredEffectMethods =
				EffectMethodsDiscovery.DiscoverEffectMethods(allCandidateMethods);

			discoveredFeatureClasses =
				FeatureClassesDiscovery.DiscoverFeatureClasses(
					allCandidateTypes: allNonAbstractCandidateTypes,
					discoveredReducerClasses: discoveredReducerClasses,
					discoveredReducerMethods: discoveredReducerMethods);
		}


		private static void GetCandidateTypes(
			IEnumerable<AssemblyScanSettings> assembliesToScan,
			IEnumerable<AssemblyScanSettings> scanIncludeList,
			IEnumerable<DiscoveredMiddleware> discoveredMiddlewares,
			out Type[] allCandidateTypes,
			out Type[] allNonAbstractCandidateTypes)
		{
			Assembly[] allCandidateAssemblies =
				assembliesToScan
					.Select(x => x.Assembly)
					.Union(scanIncludeList.Select(x => x.Assembly))
					.Distinct()
					.ToArray();

			AssemblyScanSettings[] autoLoadedMiddlewareLocations = discoveredMiddlewares
				.Where(x => x.AutoLoaded)
				.Select(x => x.ScanSettings)
				.Distinct()
				.ToArray();

			AssemblyScanSettings[] scanExcludeList = discoveredMiddlewares
				.Where(x => !x.AutoLoaded)
				.Select(x => x.ScanSettings)
				.Except(autoLoadedMiddlewareLocations)
				.Distinct()
				.ToArray();

			allCandidateTypes = AssemblyScanSettings.FilterClasses(
				scanExcludeList: scanExcludeList,
				scanIncludeList: scanIncludeList,
				types:
					allCandidateAssemblies
						.SelectMany(x => x.GetTypes())
						.Union(scanIncludeList.SelectMany(x => x.Assembly.GetTypes()))
						.Where(t => !t.IsGenericType)
						.Distinct()
						.ToArray());
			allNonAbstractCandidateTypes = allCandidateTypes
					.Where(t => !t.IsAbstract)
					.ToArray();
		}

		private static void RegisterStore(
			IServiceCollection serviceCollection,
			Options options,
			IEnumerable<DiscoveredMiddleware> discoveredMiddlewares,
			IEnumerable<DiscoveredFeatureClass> discoveredFeatureClasses,
			IEnumerable<DiscoveredEffectClass> discoveredEffectClasses,
			IEnumerable<DiscoveredEffectMethod> discoveredEffectMethods)
		{
			// Register IDispatcher as an alias to Store
			serviceCollection.AddScoped<IDispatcher, Dispatcher>();
			// Register IActionSubscriber as an alias to Store
			serviceCollection.AddScoped<IActionSubscriber>(serviceProvider => serviceProvider.GetRequiredService<Store>());
			// Register IStore as an alias to Store
			serviceCollection.AddScoped<IStore>(serviceProvider => serviceProvider.GetRequiredService<Store>());

			// Register a custom factory for building IStore that will inject all effects
			serviceCollection.AddScoped(typeof(Store), serviceProvider =>
			{
				var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
				var store = new Store(dispatcher);
				foreach (DiscoveredFeatureClass discoveredFeatureClass in discoveredFeatureClasses)
				{
					var feature = (IFeature)serviceProvider.GetRequiredService(discoveredFeatureClass.FeatureInterfaceGenericType);
					store.AddFeature(feature);
				}

				foreach (DiscoveredEffectClass discoveredEffectClass in discoveredEffectClasses)
				{
					var effect = (IEffect)serviceProvider.GetRequiredService(discoveredEffectClass.ImplementingType);
					store.AddEffect(effect);
				}

				foreach (DiscoveredEffectMethod discoveredEffectMethod in discoveredEffectMethods)
				{
					IEffect effect = EffectWrapperFactory.Create(serviceProvider, discoveredEffectMethod);
					store.AddEffect(effect);
				}

				Type[] autoLoadedMiddlewareTypes = discoveredMiddlewares
					.Where(x => x.AutoLoaded)
					.Select(x => x.ImplementingType)
					.Distinct()
					.ToArray();

				Type[] middlewareTypes = options.MiddlewareTypes
					.Union(autoLoadedMiddlewareTypes)
					.Distinct()
					.ToArray();

				foreach (Type middlewareType in middlewareTypes)
				{
					var middleware = (IMiddleware)serviceProvider.GetRequiredService(middlewareType);
					store.AddMiddleware(middleware);
				}

				return store;
			});
		}
	}
}
