using Fluxor.DependencyInjection.Wrappers;
using System;

namespace Fluxor.DependencyInjection.WrapperFactories;

internal static class EffectWrapperFactory
{
	internal static IEffect Create(
		IServiceProvider serviceProvider,
		EffectMethodInfo info)
	{
		Type actionType = info.ActionType;

		Type hostClassType = info.HostClassType;
		object effectHostInstance = info.MethodInfo.IsStatic
			? null
			: serviceProvider.GetService(hostClassType);

		Type classGenericType = typeof(EffectWrapper<>).MakeGenericType(actionType);
		var result = (IEffect)Activator.CreateInstance(
			classGenericType,
			effectHostInstance,
			info);
		return result;
	}
}
