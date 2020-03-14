using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Fluxor.DependencyInjection
{
	internal static class EffectWrapperFactory
	{
		internal static IEffect Create(IServiceProvider serviceProvider, DiscoveredEffectMethod discoveredEffectMethod)
		{
			ValidateMethod(discoveredEffectMethod.MethodInfo);
			Type actionType = discoveredEffectMethod.ActionType;

			Type hostClassType = discoveredEffectMethod.HostClassType;
			object effectHostInstance = discoveredEffectMethod.MethodInfo.IsStatic
				? null
				: serviceProvider.GetService(hostClassType);

			Type classGenericType = typeof(EffectWrapper<>).MakeGenericType(actionType);
			var result = (IEffect)Activator.CreateInstance(
				classGenericType,
				effectHostInstance,
				discoveredEffectMethod.MethodInfo);
			return result;
		}

		private static bool ValidateMethod(MethodInfo methodInfo)
		{
			if (methodInfo == null)
				throw new ArgumentNullException(nameof(methodInfo));

			ParameterInfo[] parameters = methodInfo.GetParameters();
			if (parameters.Length != 2
				|| !typeof(IDispatcher).IsAssignableFrom(parameters[1].ParameterType)
				|| methodInfo.ReturnType != typeof(Task))
			{
				throw new InvalidOperationException(
					$"{nameof(EffectMethodAttribute)} can only decorate methods in the format\r\n" +
					"public Task {NameOfMethod}({TypeOfAction} action, IDispatcher dispatcher)");
			}
			return true;
		}
	}
}
