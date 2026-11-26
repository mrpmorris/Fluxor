using Fluxor.DependencyInjection.Generated;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fluxor.DependencyInjection.InfoFactories;

internal class FeatureStateInfoFactory
{
	internal static FeatureStateInfo[] Create(
		IServiceCollection services,
		IEnumerable<Type> allCandidateTypes,
		GeneratedRegistrationCatalog catalog)
	=>
		allCandidateTypes
			.Select(x => Create(x, catalog))
			.Where(x => x is not null)
			.ToArray();

	private static FeatureStateInfo Create(Type type, GeneratedRegistrationCatalog catalog)
	{
		if (catalog.TryGetEntry(type, out GeneratedTypeEntry entry))
			return entry.FeatureState is null
				? null
				: new FeatureStateInfo(entry.FeatureState);

		FeatureStateAttribute featureStateAttribute = type.GetCustomAttribute<FeatureStateAttribute>();
		return featureStateAttribute is null
			? null
			: new FeatureStateInfo(featureStateAttribute, type);
	}
}
