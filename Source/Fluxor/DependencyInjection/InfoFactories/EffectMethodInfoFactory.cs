using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fluxor.DependencyInjection.InfoFactories
{
	internal static class EffectMethodInfoFactory
	{
		internal static EffectMethodInfo[] Create(
			IServiceCollection serviceCollection,
			IEnumerable<TypeAndMethodInfo> allCandidateMethods)
		{
			EffectMethodInfo[] discoveredEffects =
				allCandidateMethods
					.Select(c =>
						new
						{
							HostClassType = c.Type,
							c.MethodInfo,
							EffectAttribute = c.MethodInfo.GetCustomAttribute<EffectMethodAttribute>(false)
						})
					.Where(x => x.EffectAttribute != null)
					.Select(x =>
						new EffectMethodInfo(
							x.HostClassType,
							x.EffectAttribute, 
							x.MethodInfo))
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
