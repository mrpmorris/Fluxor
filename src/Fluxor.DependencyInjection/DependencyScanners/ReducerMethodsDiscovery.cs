using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fluxor.DependencyInjection.DependencyScanners
{
	internal static class ReducerMethodsDiscovery
	{
		internal static IEnumerable<DiscoveredReducerMethod> DiscoverReducerMethods(
			IServiceCollection serviceCollection,
			IEnumerable<Type> allCandidateTypes)
		{
			var discoveredReducers = allCandidateTypes
				.SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
				.Select(m => new
				{
					MethodInfo = m,
					ReducerAttribute = m.GetCustomAttribute<ReducerMethodAttribute>(false)
				})
				.Where(x => x.ReducerAttribute != null)
				.Select(x => new DiscoveredReducerMethod(
					hostClassType: x.MethodInfo.DeclaringType,
					methodInfo: x.MethodInfo,
					stateType: x.MethodInfo.GetParameters()[0].ParameterType,
					actionType: x.MethodInfo.GetParameters()[1].ParameterType));

			IEnumerable<Type> hostClassTypes = discoveredReducers
				.Select(x => x.HostClassType)
				.Where(t => !t.IsAbstract)
				.Distinct();

			foreach (Type hostClassType in hostClassTypes)
				if (!hostClassType.IsAbstract)
					serviceCollection.AddScoped(hostClassType);

			return discoveredReducers;
		}
	}
}
