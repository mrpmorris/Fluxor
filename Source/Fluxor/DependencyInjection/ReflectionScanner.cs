using Fluxor.DependencyInjection.InfoFactories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fluxor.DependencyInjection
{
	internal static class ReflectionScanner
	{
		internal static void Scan(
			this IServiceCollection serviceCollection,
			FluxorOptions options,
			IEnumerable<Type> typesToScan,
			IEnumerable<AssemblyScanSettings> assembliesToScan,
			IEnumerable<AssemblyScanSettings> scanIncludeList)
		{
			int totalScanSources = 0;
			totalScanSources += assembliesToScan?.Count() ?? 0;
			totalScanSources += typesToScan?.Count() ?? 0;

			if (totalScanSources < 1)
				throw new ArgumentException($"Must supply either {typesToScan} or {assembliesToScan}");

			GetCandidateTypes(
				assembliesToScan: assembliesToScan,
				typesToScan: typesToScan,
				scanIncludeList: scanIncludeList ?? new List<AssemblyScanSettings>(),
				allCandidateTypes: out Type[] allCandidateTypes,
				allNonAbstractCandidateTypes: out Type[] allNonAbstractCandidateTypes);

			// Classes must not be abstract
			ReducerClassInfo[] reducerClassInfos =
				ReducerClassInfoFactory.Create(serviceCollection, allNonAbstractCandidateTypes);

			EffectClassInfo[] effectClassInfos =
				EffectClassInfoFactory.Create(serviceCollection, allNonAbstractCandidateTypes);

			// Method reducer/effects may belong to abstract classes
			TypeAndMethodInfo[] allCandidateMethods = AssemblyScanSettings.FilterMethods(allCandidateTypes);

			ReducerMethodInfo[] reducerMethodInfos =
				ReducerMethodInfoFactory.Create(serviceCollection, allCandidateMethods);

			EffectMethodInfo[] effectMethodInfos =
				EffectMethodInfoFactory.Create(serviceCollection, allCandidateMethods);

			FeatureClassInfo[] featureClassInfos =
				FeatureClassInfoFactory.Create(
					serviceCollection: serviceCollection,
					allCandidateTypes: allNonAbstractCandidateTypes,
					reducerClassInfos: reducerClassInfos,
					reducerMethodInfos: reducerMethodInfos);

			RegisterStore(
				serviceCollection: serviceCollection,
				options: options,
				featureClassInfos: featureClassInfos,
				effectClassInfos: effectClassInfos,
				effectMethodInfos: effectMethodInfos);
		}

		private static void GetCandidateTypes(
			IEnumerable<AssemblyScanSettings> assembliesToScan,
			IEnumerable<Type> typesToScan,
			IEnumerable<AssemblyScanSettings> scanIncludeList,
			out Type[] allCandidateTypes,
			out Type[] allNonAbstractCandidateTypes)
		{
			Assembly[] allCandidateAssemblies =
				assembliesToScan
					.Select(x => x.Assembly)
					.Union(scanIncludeList.Select(x => x.Assembly))
					.Distinct()
					.ToArray();

			AssemblyScanSettings[] scanExcludeList =
				MiddlewareClassesDiscovery.FindMiddlewareLocations(allCandidateAssemblies);

			allCandidateTypes = AssemblyScanSettings.FilterClasses(
				scanExcludeList: scanExcludeList,
				scanIncludeList: scanIncludeList,
				types:
					allCandidateAssemblies
						.SelectMany(x => x.GetTypes())
						.Union(scanIncludeList.SelectMany(x => x.Assembly.GetTypes()))
						.Where(t => !t.IsGenericTypeDefinition)
						.Distinct()
						.ToArray()
				)
				.Union(typesToScan)
				.ToArray();
			allNonAbstractCandidateTypes = allCandidateTypes
					.Where(t => !t.IsAbstract)
					.ToArray();
		}

		private static void RegisterStore(
			IServiceCollection serviceCollection,
			FluxorOptions options,
			IEnumerable<FeatureClassInfo> featureClassInfos,
			IEnumerable<EffectClassInfo> effectClassInfos,
			IEnumerable<EffectMethodInfo> effectMethodInfos)
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
				foreach (FeatureClassInfo featureClassInfo in featureClassInfos)
				{
					var feature = (IFeature)serviceProvider.GetService(featureClassInfo.FeatureInterfaceGenericType);
					store.AddFeature(feature);
				}

				foreach (EffectClassInfo effectClassInfo in effectClassInfos)
				{
					var effect = (IEffect)serviceProvider.GetService(effectClassInfo.ImplementingType);
					store.AddEffect(effect);
				}

				foreach (EffectMethodInfo effectMethodInfo in effectMethodInfos)
				{
					IEffect effect = EffectWrapperFactory.Create(serviceProvider, effectMethodInfo);
					store.AddEffect(effect);
				}

				foreach (Type middlewareType in options.MiddlewareTypes)
				{
					var middleware = (IMiddleware)serviceProvider.GetService(middlewareType);
					store.AddMiddleware(middleware);
				}

				return store;
			});
		}
	}
}
