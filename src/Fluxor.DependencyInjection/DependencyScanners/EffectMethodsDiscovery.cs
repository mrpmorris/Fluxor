using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fluxor.DependencyInjection.DependencyScanners
{
	internal static class EffectMethodsDiscovery
	{
		internal static IEnumerable<DiscoveredEffectMethod> DiscoverEffectMethods(IServiceCollection serviceCollection,
			IEnumerable<Type> allCandidateTypes)
		{
			var discoveredEffects = allCandidateTypes
				.SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
				.Select(m => new
				{
					MethodInfo = m,
					EffectAttribute = m.GetCustomAttribute<EffectMethodAttribute>(false)
				})
				.Where(x => x.EffectAttribute != null)
				.Select(x => new DiscoveredEffectMethod(
					hostClassType: x.MethodInfo.DeclaringType,
					methodInfo: x.MethodInfo,
					actionType: x.MethodInfo.GetParameters()[0].ParameterType));

			IEnumerable<Type> hostClassTypes = discoveredEffects
				.Select(x => x.HostClassType)
				.Where(t => !t.IsAbstract)
				.Distinct();

			foreach (Type hostClassType in hostClassTypes)
				if (!hostClassType.IsAbstract)
					serviceCollection.AddScoped(hostClassType);

			return discoveredEffects;
		}
	}
}
