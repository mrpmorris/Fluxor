using Fluxor.DependencyInjection.Wrappers;
using System;

namespace Fluxor.DependencyInjection.WrapperFactories
{
	internal static class ReducerWrapperFactory
	{
		internal static object Create(
			IServiceProvider serviceProvider,
			ReducerMethodInfo info)
		{
			Type stateType = info.StateType;
			Type actionType = info.ActionType;

			Type hostClassType = info.HostClassType;
			object reducerHostInstance = info.MethodInfo.IsStatic
				? null
				: serviceProvider.GetService(hostClassType);

			Type classGenericType = typeof(ReducerWrapper<,>).MakeGenericType(stateType, actionType);
			var result = Activator.CreateInstance(
				classGenericType,
				reducerHostInstance,
				info);
			return result;
		}
	}
}
