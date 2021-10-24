using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluxor.DependencyInjection.DependencyScanners
{
	internal static class EffectClassessDiscovery
	{
		internal static EffectClassInfo[] DiscoverEffectClasses(
			IServiceCollection serviceCollection, IEnumerable<Type> allCandidateTypes)
		{
			EffectClassInfo[] discoveredEffectInfos =
				allCandidateTypes
					.Where(t => typeof(IEffect).IsAssignableFrom(t))
					.Where(t => t != typeof(EffectWrapper<>))
					.Select(t => new EffectClassInfo(implementingType: t))
					.ToArray();

			foreach (EffectClassInfo discoveredEffectInfo in discoveredEffectInfos)
				serviceCollection.AddScoped(discoveredEffectInfo.ImplementingType);

			return discoveredEffectInfos;
		}
	}
}
