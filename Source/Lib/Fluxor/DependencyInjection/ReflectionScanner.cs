using Fluxor.DependencyInjection.InfoFactories;
using Fluxor.DependencyInjection.ServiceRegistration;
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
			this IServiceCollection services,
			FluxorOptions options,
			IEnumerable<IFluxorModule> modulesToImport,
			IEnumerable<Type> typesToScan,
			IEnumerable<AssemblyScanSettings> assembliesToScan,
			IEnumerable<AssemblyScanSettings> scanIncludeList)
		{
			modulesToImport ??= Array.Empty<IFluxorModule>();
			typesToScan ??= Array.Empty<Type>();
			assembliesToScan ??= Array.Empty<AssemblyScanSettings>();

			int totalScanSources = 0;
			totalScanSources += assembliesToScan.Count();
			totalScanSources += typesToScan.Count();
			totalScanSources += modulesToImport.Count();

			if (totalScanSources < 1)
				throw new ArgumentException(
					$"Must supply either {nameof(typesToScan)}, {nameof(assembliesToScan)}, or {nameof(modulesToImport)}");

			GetCandidateTypes(
				assembliesToScan: assembliesToScan,
				typesToScan: typesToScan,
				modulesToImport: modulesToImport,
				scanIncludeList: scanIncludeList ?? new List<AssemblyScanSettings>(),
				allCandidateTypes: out Type[] allCandidateTypes,
				allNonAbstractCandidateTypes: out Type[] allNonAbstractCandidateTypes);

			// Method reducer/effects may belong to abstract classes
			TypeAndMethodInfo[] allCandidateMethods =
				AssemblyScanSettings.FilterMethods(allCandidateTypes);

			// Find all concrete implementors of IReducer<T>
			ReducerClassInfo[] reducerClassInfos =
				ReducerClassInfoFactory.Create(services, allNonAbstractCandidateTypes);

			// Find all [ReducerMethod] decorated methods
			ReducerMethodInfo[] reducerMethodInfos =
				ReducerMethodInfoFactory.Create(services, allCandidateMethods);

			// Find all concrete implementors of IEffect<T>
			EffectClassInfo[] effectClassInfos =
				EffectClassInfoFactory.Create(services, allNonAbstractCandidateTypes);

			// Find all [EffectMethod] decorated methods
			EffectMethodInfo[] effectMethodInfos =
				EffectMethodInfoFactory.Create(services, allCandidateMethods);

			// Find all concrete implementors of IFeature
			FeatureClassInfo[] featureClassInfos =
				FeatureClassInfoFactory.Create(
					services: services,
					allCandidateTypes: allNonAbstractCandidateTypes,
					reducerClassInfos: reducerClassInfos,
					reducerMethodInfos: reducerMethodInfos);

			FeatureStateInfo[] featureStateInfos =
				FeatureStateInfoFactory.Create(
					services: services,
					allCandidateTypes: allCandidateTypes);

			StoreRegistration.Register(
				services,
				options,
				featureClassInfos,
				featureStateInfos,
				reducerClassInfos,
				reducerMethodInfos,
				effectClassInfos,
				effectMethodInfos);
		}

		private static void GetCandidateTypes(
			IEnumerable<AssemblyScanSettings> assembliesToScan,
			IEnumerable<Type> typesToScan,
			IEnumerable<AssemblyScanSettings> scanIncludeList,
			IEnumerable<IFluxorModule> modulesToImport,
			out Type[] allCandidateTypes,
			out Type[] allNonAbstractCandidateTypes)
		{
			Assembly[] allCandidateAssemblies =
				assembliesToScan
					.Select(x => x.Assembly)
					.Union(scanIncludeList.Select(x => x.Assembly))
					.Distinct()
					.ToArray();

			AssemblyAndNamespace[] scanExcludeList =
				MiddlewareClassesDiscovery.FindMiddlewareLocations(
					allCandidateAssemblies,
					modulesToImport);

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
	}
}
