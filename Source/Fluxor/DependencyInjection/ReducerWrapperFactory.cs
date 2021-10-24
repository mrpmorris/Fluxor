using System;

namespace Fluxor.DependencyInjection
{
	internal static class ReducerWrapperFactory
	{
		internal static object Create(
			IServiceProvider serviceProvider,
			ReducerMethodInfo reducerMethodInfo)
		{
			Type stateType = reducerMethodInfo.StateType;
			Type actionType = reducerMethodInfo.ActionType;

			Type hostClassType = reducerMethodInfo.HostClassType;
			object reducerHostInstance = reducerMethodInfo.MethodInfo.IsStatic
				? null
				: serviceProvider.GetService(hostClassType);

			Type classGenericType = typeof(ReducerWrapper<,>).MakeGenericType(stateType, actionType);
			var result = Activator.CreateInstance(
				classGenericType,
				reducerHostInstance,
				reducerMethodInfo);
			return result;
		}
	}
}
