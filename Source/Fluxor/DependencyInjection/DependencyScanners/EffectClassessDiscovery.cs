using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluxor.DependencyInjection.DependencyScanners
{
	internal static class EffectClassessDiscovery
	{
		internal static DiscoveredEffectClass[] DiscoverEffectClasses(
			IEnumerable<Type> allCandidateTypes)
		=>
			allCandidateTypes
				.Where(t => typeof(IEffect).IsAssignableFrom(t))
				.Where(t => t != typeof(EffectWrapper<>))
				.Select(t => new DiscoveredEffectClass(implementingType: t))
				.ToArray();
	}
}
