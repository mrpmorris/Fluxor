using Fluxor.DependencyInjection.Generated;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluxor.DependencyInjection.InfoFactories;

internal static class FeatureClassInfoFactory
{
	internal static FeatureClassInfo[] Create(
		IServiceCollection services,
		IEnumerable<Type> allCandidateTypes,
		IEnumerable<ReducerClassInfo> reducerClassInfos,
		IEnumerable<ReducerMethodInfo> reducerMethodInfos,
		GeneratedRegistrationCatalog catalog)
	=>
		allCandidateTypes
			.Select(t => Create(t, catalog))
			.Where(x => x is not null)
			.ToArray();

	private static FeatureClassInfo Create(Type type, GeneratedRegistrationCatalog catalog)
	{
		if (catalog.TryGetEntry(type, out GeneratedTypeEntry entry))
			return entry.FeatureClass is null
				? null
				: new FeatureClassInfo(implementingType: type, entry.FeatureClass);

		Type[] genericParameterTypes = TypeHelper.GetGenericParametersForImplementedInterface(type, typeof(IFeature<>));
		return genericParameterTypes is null
			? null
			: new FeatureClassInfo(implementingType: type, stateType: genericParameterTypes[0]);
	}
}
