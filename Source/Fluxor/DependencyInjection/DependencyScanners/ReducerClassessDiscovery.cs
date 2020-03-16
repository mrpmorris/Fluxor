using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluxor.DependencyInjection.DependencyScanners
{
	internal static class ReducerClassessDiscovery
	{
		internal static IEnumerable<DiscoveredReducerClass> DiscoverReducerClasses(
			IServiceCollection serviceCollection, IEnumerable<Type> allCandidateTypes)
		{
			IEnumerable<DiscoveredReducerClass> discoveredReducerInfos = allCandidateTypes
				.Where(t => t != typeof(ReducerWrapper<,>))
				.Select(t => new
				{
					ImplementingType = t,
					GenericParameterTypes = TypeHelper.GetGenericParametersForImplementedInterface(t, typeof(IReducer<>))
				})
				.Where(x => x.GenericParameterTypes != null)
				.Select(x => new DiscoveredReducerClass(
					implementingType: x.ImplementingType,
					stateType: x.GenericParameterTypes[0]))
				.ToList();

			foreach (DiscoveredReducerClass discoveredReducerInfo in discoveredReducerInfos)
				serviceCollection.AddScoped(serviceType: discoveredReducerInfo.ImplementingType);

			return discoveredReducerInfos;
		}
	}
}
