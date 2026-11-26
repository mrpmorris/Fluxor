using Fluxor.DependencyInjection.Wrappers;
using System;

namespace Fluxor.DependencyInjection.WrapperFactories;

internal static class ReducerWrapperFactory
{
	internal static object Create(
		IServiceProvider serviceProvider,
		ReducerMethodInfo info)
	{
		Type stateType = info.StateType;
		Type actionType = info.ActionType;

		Type hostClassType = info.HostClassType;
		object reducerHostInstance = info.IsStatic
			? null
			: serviceProvider.GetService(hostClassType);

		if (info.CreateReducer is not null)
			return info.CreateReducer(reducerHostInstance, info.GeneratedIndex);

		Type classGenericType = typeof(ReducerWrapper<,>).MakeGenericType(stateType, actionType);
		var result = Activator.CreateInstance(
			classGenericType,
			reducerHostInstance,
			info);
		return result;
	}
}
