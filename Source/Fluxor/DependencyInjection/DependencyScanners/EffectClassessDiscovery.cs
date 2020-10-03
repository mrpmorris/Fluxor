using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluxor.DependencyInjection.DependencyScanners
{
	internal static class EffectClassessDiscovery
	{
		internal static DiscoveredEffectClass[] DiscoverEffectClasses(
			IServiceCollection serviceCollection, IEnumerable<Type> allCandidateTypes)
		{
			DiscoveredEffectClass[] discoveredEffectInfos =
				allCandidateTypes
					.Where(t => typeof(IEffect).IsAssignableFrom(t))
					.Where(t => t != typeof(EffectWrapper<>))
					.Select(t => new DiscoveredEffectClass(implementingType: t))
					.ToArray();

			foreach (DiscoveredEffectClass discoveredEffectInfo in discoveredEffectInfos)
				serviceCollection.AddScoped(discoveredEffectInfo.ImplementingType);

			return discoveredEffectInfos;
		}
	}
}
