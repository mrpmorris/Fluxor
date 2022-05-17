using Fluxor.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluxor.DependencyInjection.ServiceRegistration
{
	internal static class EffectMethodRegistration
	{
		public static void Register(
			IServiceCollection services,
			EffectMethodInfo[] effectMethodInfos,
			FluxorOptions options)
		{
			IEnumerable<Type> hostClassTypes =
				effectMethodInfos
					.Select(x => x.HostClassType)
					.Where(t => !t.IsAbstract)
					.Distinct();

			foreach (Type hostClassType in hostClassTypes)
				services.Add(hostClassType, options);
		}
	}
}
