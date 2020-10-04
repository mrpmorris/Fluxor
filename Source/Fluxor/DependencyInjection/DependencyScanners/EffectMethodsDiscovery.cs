using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fluxor.DependencyInjection.DependencyScanners
{
	internal static class EffectMethodsDiscovery
	{
		internal static DiscoveredEffectMethod[] DiscoverEffectMethods(IServiceCollection serviceCollection,
			IEnumerable<MethodInfo> allCandidateMethods)
		{
			DiscoveredEffectMethod[] discoveredEffects =
				allCandidateMethods
					.Select(m =>
						new
						{
							MethodInfo = m,
							EffectAttribute = m.GetCustomAttribute<EffectMethodAttribute>(false)
						})
					.Where(x => x.EffectAttribute != null)
					.Select(x => new DiscoveredEffectMethod(x.EffectAttribute, x.MethodInfo))
					.ToArray();

			IEnumerable<Type> hostClassTypes =
				discoveredEffects
					.Select(x => x.HostClassType)
					.Where(t => !t.IsAbstract)
					.Distinct();

			foreach (Type hostClassType in hostClassTypes)
				serviceCollection.AddScoped(hostClassType);

			return discoveredEffects;
		}
	}
}
