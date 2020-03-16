using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluxor.DependencyInjection.DependencyScanners
{
	internal static class EffectClassessDiscovery
	{
		internal static IEnumerable<DiscoveredEffectClass> DiscoverEffectClasses(
			IServiceCollection serviceCollection, IEnumerable<Type> allCandidateTypes)
		{
			IEnumerable<DiscoveredEffectClass> discoveredEffectInfos = allCandidateTypes
				.Where(t => typeof(IEffect).IsAssignableFrom(t))
				.Where(t => t != typeof(EffectWrapper<>))
				.Select(t => new DiscoveredEffectClass(implementingType: t))
				.ToList();

			foreach (DiscoveredEffectClass discoveredEffectInfo in discoveredEffectInfos)
				serviceCollection.AddScoped(discoveredEffectInfo.ImplementingType);

			return discoveredEffectInfos;
		}
	}
}
