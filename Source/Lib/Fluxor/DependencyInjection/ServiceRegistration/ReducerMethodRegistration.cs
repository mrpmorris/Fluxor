using Fluxor.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluxor.DependencyInjection.ServiceRegistration;

internal static class ReducerMethodRegistration
{
	public static IEnumerable<Type> GetHostClassTypes(ReducerMethodInfo[] reducerMethodInfos) =>
		reducerMethodInfos
			.Where(x => !x.MethodInfo.IsStatic)
			.Select(x => x.HostClassType)
			.Where(t => !t.IsAbstract)
			.Distinct();
}
