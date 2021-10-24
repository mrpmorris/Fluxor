using System;

namespace Fluxor.DependencyInjection
{
	internal static class EffectWrapperFactory
	{
		internal static IEffect Create(
			IServiceProvider serviceProvider,
			EffectMethodInfo effectMethodInfo)
		{
			Type actionType = effectMethodInfo.ActionType;

			Type hostClassType = effectMethodInfo.HostClassType;
			object effectHostInstance = effectMethodInfo.MethodInfo.IsStatic
				? null
				: serviceProvider.GetService(hostClassType);

			Type classGenericType = typeof(EffectWrapper<>).MakeGenericType(actionType);
			var result = (IEffect)Activator.CreateInstance(
				classGenericType,
				effectHostInstance,
				effectMethodInfo);
			return result;
		}
	}
}
