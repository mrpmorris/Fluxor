using Fluxor.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluxor.DependencyInjection.ServiceRegistration;

internal static class EffectMethodRegistration
{
	public static IEnumerable<Type> GetHostClassTypes(EffectMethodInfo[] effectMethodInfos) =>
		effectMethodInfos
			.Where(x => !x.MethodInfo.IsStatic)
			.Select(x => x.HostClassType)
			.Where(t => !t.IsAbstract)
			.Distinct();
}
