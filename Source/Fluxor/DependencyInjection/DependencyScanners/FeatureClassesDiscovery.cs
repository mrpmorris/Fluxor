using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fluxor.DependencyInjection.DependencyScanners
{
	internal static class FeatureClassesDiscovery
	{
		internal static DiscoveredFeatureClass[] DiscoverFeatureClasses(IEnumerable<Type> allCandidateTypes)
		=>
			allCandidateTypes
				.Select(t =>
					new
					{
						ImplementingType = t,
						GenericParameterTypes = TypeHelper.GetGenericParametersForImplementedInterface(t, typeof(IFeature<>))
					})
				.Where(x => x.GenericParameterTypes != null)
				.Select(x => new DiscoveredFeatureClass(
					implementingType: x.ImplementingType,
					stateType: x.GenericParameterTypes[0]
					)
				)
				.ToArray();
	}
}
