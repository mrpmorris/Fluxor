using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fluxor.DependencyInjection.DependencyScanners
{
	internal static class ReducerMethodsDiscovery
	{
		internal static DiscoveredReducerMethod[] DiscoverReducerMethods(
			IEnumerable<TypeAndMethodInfo> allCandidateMethods)
		=>
			allCandidateMethods
				.Select(c =>
					new
					{
						HostClassType = c.Type,
						c.MethodInfo,
						ReducerAttribute = c.MethodInfo.GetCustomAttribute<ReducerMethodAttribute>(false)
					})
				.Where(x => x.ReducerAttribute != null)
				.Select(x => new DiscoveredReducerMethod(
					x.HostClassType,
					x.ReducerAttribute,
					x.MethodInfo))
				.ToArray();
	}
}
