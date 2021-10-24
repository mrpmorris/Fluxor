using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace Fluxor.DependencyInjection.ServiceRegistration
{
	internal static class EffectClassRegistration
	{
		public static void Register(
			IServiceCollection services,
			IEnumerable<EffectClassInfo> effectClassInfos)
		{
			foreach (EffectClassInfo effectClassInfo in effectClassInfos)
				services.AddScoped(effectClassInfo.ImplementingType);
		}
	}
}
