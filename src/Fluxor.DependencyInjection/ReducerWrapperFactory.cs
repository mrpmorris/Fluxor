using System;
using System.Reflection;

namespace Fluxor.DependencyInjection
{
	internal static class ReducerWrapperFactory
	{
		internal static object Create(IServiceProvider serviceProvider, DiscoveredReducerMethod discoveredReducerMethod)
		{
			ValidateMethod(discoveredReducerMethod.MethodInfo);
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
				discoveredReducerMethod.MethodInfo);
			return result;
		}

		private static bool ValidateMethod(MethodInfo methodInfo)
		{
			if (methodInfo == null)
				throw new ArgumentNullException(nameof(methodInfo));

			ParameterInfo[] parameters = methodInfo.GetParameters();
			if (parameters.Length != 2
				|| methodInfo.ReturnType != parameters[0].ParameterType)
			{
				throw new InvalidOperationException(
					$"{nameof(ReducerMethodAttribute)} can only decorate methods in the format\r\n" +
					"public {TypeOfState} {NameOfMethod}({TypeOfState} state, {TypeOfAction} action)");
			}
			return true;
		}
	}
}
