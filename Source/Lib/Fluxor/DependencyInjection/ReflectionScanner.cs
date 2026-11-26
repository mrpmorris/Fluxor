using Fluxor.DependencyInjection.InfoFactories;
using Fluxor.DependencyInjection.ServiceRegistration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fluxor.DependencyInjection;

internal static class ReflectionScanner
{
	internal static void Scan(
		this IServiceCollection services,
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

		scanIncludeList ??= new List<AssemblyScanSettings>();
		typesToScan ??= Array.Empty<Type>();

		// Assemblies with a generated registration are described by it instead of being scanned
		GeneratedRegistrationCatalog catalog = GeneratedRegistrationCatalog.Create(
			GetCandidateAssemblies(assembliesToScan, scanIncludeList)
				.Union(typesToScan.Select(x => x.Assembly)));

		GetCandidateTypes(
			assembliesToScan: assembliesToScan,
			typesToScan: typesToScan,
			scanIncludeList: scanIncludeList,
			catalog: catalog,
			allCandidateTypes: out Type[] allCandidateTypes,
			allNonAbstractCandidateTypes: out Type[] allNonAbstractCandidateTypes);

		// Method reducer/effects may belong to abstract classes
		TypeAndMethodInfo[] allCandidateMethods =
			AssemblyScanSettings.FilterMethods(allCandidateTypes, catalog);

		// Find all concrete implementors of IReducer<T>
		ReducerClassInfo[] reducerClassInfos =
			ReducerClassInfoFactory.Create(services, allNonAbstractCandidateTypes, catalog);

		// Find all [ReducerMethod] decorated methods
		ReducerMethodInfo[] reducerMethodInfos =
			ReducerMethodInfoFactory.Create(services, allCandidateMethods);

		// Find all concrete implementors of IEffect<T>
		EffectClassInfo[] effectClassInfos =
			EffectClassInfoFactory.Create(services, allNonAbstractCandidateTypes, catalog);

		// Find all [EffectMethod] decorated methods
		EffectMethodInfo[] effectMethodInfos =
			EffectMethodInfoFactory.Create(services, allCandidateMethods);

		// Find all concrete implementors of IFeature
		FeatureClassInfo[] featureClassInfos =
			FeatureClassInfoFactory.Create(
				services: services,
				allCandidateTypes: allNonAbstractCandidateTypes,
				reducerClassInfos: reducerClassInfos,
				reducerMethodInfos: reducerMethodInfos,
				catalog: catalog);

		FeatureStateInfo[] featureStateInfos =
			FeatureStateInfoFactory.Create(
				services: services,
				allCandidateTypes: allCandidateTypes,
				catalog: catalog);

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

	private static Assembly[] GetCandidateAssemblies(
		IEnumerable<AssemblyScanSettings> assembliesToScan,
		IEnumerable<AssemblyScanSettings> scanIncludeList)
		=>
			assembliesToScan
				.Select(x => x.Assembly)
				.Union(scanIncludeList.Select(x => x.Assembly))
				.Distinct()
				.ToArray();

	private static void GetCandidateTypes(
		IEnumerable<AssemblyScanSettings> assembliesToScan,
		IEnumerable<Type> typesToScan,
		IEnumerable<AssemblyScanSettings> scanIncludeList,
		GeneratedRegistrationCatalog catalog,
		out Type[] allCandidateTypes,
		out Type[] allNonAbstractCandidateTypes)
	{
		Assembly[] allCandidateAssemblies = GetCandidateAssemblies(assembliesToScan, scanIncludeList);

		AssemblyScanSettings[] scanExcludeList =
			MiddlewareClassesDiscovery.FindMiddlewareLocations(allCandidateAssemblies, catalog);

		allCandidateTypes = AssemblyScanSettings.FilterClasses(
			scanExcludeList: scanExcludeList,
			scanIncludeList: scanIncludeList,
			types:
				allCandidateAssemblies
					.SelectMany(x => GetTypes(x, catalog))
					.Union(scanIncludeList.SelectMany(x => GetTypes(x.Assembly, catalog)))
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

	// An assembly with a generated registration contributes only its described types, in GetTypes() order
	private static Type[] GetTypes(Assembly assembly, GeneratedRegistrationCatalog catalog) =>
		catalog.TryGetTypes(assembly, out Type[] types)
		? types
		: assembly.GetTypes();
}
