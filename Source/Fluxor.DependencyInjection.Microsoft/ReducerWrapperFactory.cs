using System;

namespace Fluxor.DependencyInjection
{
	internal static class ReducerWrapperFactory
	{
		internal static object Create(IServiceProvider serviceProvider, DiscoveredReducerMethod discoveredReducerMethod)
		{
			Type stateType = discoveredReducerMethod.StateType;
			Type actionType = discoveredReducerMethod.ActionType;

			Type hostClassType = discoveredReducerMethod.HostClassType;
			object reducerHostInstance = discoveredReducerMethod.MethodInfo.IsStatic
				? null
				: serviceProvider.GetService(hostClassType);

			Type classGenericType = typeof(ReducerWrapper<,>).MakeGenericType(stateType, actionType);
			var result = Activator.CreateInstance(
				classGenericType,
				reducerHostInstance,
				discoveredReducerMethod);
			return result;
		}
	}
}
