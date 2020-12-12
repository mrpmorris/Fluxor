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

			var allDiscoveredMiddlewares =
				MiddlewareClassesDiscovery.FindMiddlewares(
					assembliesToScan: assemblyScanSettings.Select(x => x.Assembly),
					manuallyIncludedMiddlewares: middlewareTypes);

			GetCandidateTypes(
				assembliesToScan: assemblyScanSettings,
				scanIncludeList: scanIncludeList,
				discoveredMiddlewares: allDiscoveredMiddlewares,
				allCandidateTypes: out Type[] allCandidateTypes,
				allNonAbstractCandidateTypes: out Type[] allNonAbstractCandidateTypes);

			// Only allowed middlewares
			discoveredMiddlewares =
				allDiscoveredMiddlewares
				.Where(x => allNonAbstractCandidateTypes.Contains(x.ImplementingType))
				.ToArray();

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
				FeatureClassesDiscovery.DiscoverFeatureClasses(allNonAbstractCandidateTypes);
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
	}
}
