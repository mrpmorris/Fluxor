using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fluxor.DependencyInjection.DependencyScanners
{
	internal static class EffectMethodsDiscovery
	{
		internal static DiscoveredEffectMethod[] DiscoverEffectMethods(
			IEnumerable<TypeAndMethodInfo> allCandidateMethods)
		=>
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
					new DiscoveredEffectMethod(
						x.HostClassType,
						x.EffectAttribute,
						x.MethodInfo))
				.ToArray();
	}
}
