using Fluxor.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace Fluxor.DependencyInjection.ServiceRegistration
{
	internal static class EffectClassRegistration
	{
		public static void Register(
			IServiceCollection services,
			IEnumerable<EffectClassInfo> effectClassInfos,
			FluxorOptions options)
		{
			foreach (EffectClassInfo effectClassInfo in effectClassInfos)
				services.Add(effectClassInfo.ImplementingType, options);
		}
	}
}
