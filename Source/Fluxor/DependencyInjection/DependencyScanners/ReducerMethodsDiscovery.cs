using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fluxor.DependencyInjection.DependencyScanners
{
	internal static class ReducerMethodsDiscovery
	{
		internal static DiscoveredReducerMethod[] DiscoverReducerMethods(
			IServiceCollection serviceCollection,
			IEnumerable<TypeAndMethodInfo> allCandidateMethods)
		{
			DiscoveredReducerMethod[] discoveredReducers =
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

			IEnumerable<Type> hostClassTypes =
				discoveredReducers
					.Select(x => x.HostClassType)
					.Where(t => !t.IsAbstract)
					.Distinct();

			foreach (Type hostClassType in hostClassTypes)
				serviceCollection.AddScoped(hostClassType);

			return discoveredReducers;
		}
	}
}
