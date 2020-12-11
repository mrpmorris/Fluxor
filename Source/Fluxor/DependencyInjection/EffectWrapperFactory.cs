using System;

namespace Fluxor.DependencyInjection
{
	internal static class EffectWrapperFactory
	{
		internal static IEffect Create(ObjectBuilder objectBuilder, DiscoveredEffectMethod discoveredEffectMethod)
		{
			Type actionType = discoveredEffectMethod.ActionType;

			Type hostClassType = discoveredEffectMethod.HostClassType;
			object effectHostInstance = discoveredEffectMethod.MethodInfo.IsStatic
				? null
				: objectBuilder.Build(hostClassType);

			Type classGenericType = typeof(EffectWrapper<>).MakeGenericType(actionType);
			var result = (IEffect)Activator.CreateInstance(
				classGenericType,
				effectHostInstance,
				discoveredEffectMethod);
			return result;
		}
	}
}
