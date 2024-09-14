using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluxor.DependencyInjection.InfoFactories;

internal static class FeatureClassInfoFactory
{
	internal static FeatureClassInfo[] Create(IEnumerable<Type> allCandidateTypes)
	=>
		allCandidateTypes
			.Select(t =>
				new
				{
					ImplementingType = t,
					GenericParameterTypes = TypeHelper.GetGenericParametersForImplementedInterface(t, typeof(IFeature<>))
				})
			.Where(x => x.GenericParameterTypes is not null)
			.Select(x => new FeatureClassInfo(
				implementingType: x.ImplementingType,
				stateType: x.GenericParameterTypes[0]
				)
			)
			.ToArray();
}
