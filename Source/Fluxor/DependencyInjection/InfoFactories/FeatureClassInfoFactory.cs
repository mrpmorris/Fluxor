using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fluxor.DependencyInjection.InfoFactories
{
	internal static class FeatureClassInfoFactory
	{
		internal static FeatureClassInfo[] Create(
			IServiceCollection services,
			IEnumerable<Type> allCandidateTypes,
			IEnumerable<ReducerClassInfo> reducerClassInfos,
			IEnumerable<ReducerMethodInfo> reducerMethodInfos)
		=>
			allCandidateTypes
				.Select(t =>
					new
					{
						ImplementingType = t,
						GenericParameterTypes = TypeHelper.GetGenericParametersForImplementedInterface(t, typeof(IFeature<>))
					})
				.Where(x => x.GenericParameterTypes != null)
				.Select(x => new FeatureClassInfo(
					implementingType: x.ImplementingType,
					stateType: x.GenericParameterTypes[0]
					)
				)
				.ToArray();
	}
}
