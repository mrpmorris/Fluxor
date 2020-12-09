using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluxor.DependencyInjection.DependencyScanners
{
	internal static class ReducerClassessDiscovery
	{
		internal static DiscoveredReducerClass[] DiscoverReducerClasses(
			IEnumerable<Type> allCandidateTypes)
		=>
			allCandidateTypes
				.Where(t => t != typeof(ReducerWrapper<,>))
				.Select(t =>
					new
					{
						ImplementingType = t,
						GenericParameterTypes = TypeHelper.GetGenericParametersForImplementedInterface(t, typeof(IReducer<>))
					})
				.Where(x => x.GenericParameterTypes != null)
				.Select(x => new DiscoveredReducerClass(
					implementingType: x.ImplementingType,
					stateType: x.GenericParameterTypes[0]))
				.ToArray();
	}
}
