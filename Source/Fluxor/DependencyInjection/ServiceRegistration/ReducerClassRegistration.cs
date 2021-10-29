using Microsoft.Extensions.DependencyInjection;

namespace Fluxor.DependencyInjection.ServiceRegistration
{
	internal static class ReducerClassRegistration
	{
		public static void Register(
			IServiceCollection services,
			ReducerClassInfo[] reducerClassInfos)
		{
			foreach (ReducerClassInfo reducerClassInfo in reducerClassInfos)
				services.AddScoped(serviceType: reducerClassInfo.ImplementingType);
		}
	}
}
