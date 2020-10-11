using Fluxor.Extensions;
using System;
using System.Reflection;

namespace Fluxor.DependencyInjection
{
	internal class DiscoveredReducerMethod
	{
		public readonly Type HostClassType;
		public readonly MethodInfo MethodInfo;
		public readonly Type StateType;
		public readonly Type ActionType;
		public readonly bool RequiresActionParameterInMethod;

		public DiscoveredReducerMethod(Type hostClassType, ReducerMethodAttribute attribute, MethodInfo methodInfo)
		{
			ParameterInfo[] methodParameters = methodInfo.GetParameters();
			if (attribute.ActionType == null && methodParameters.Length != 2)
				throw new ArgumentException(
					$"Method must have 2 parameters (state, action)"
						+ $" when [{nameof(ReducerMethodAttribute)}] has no {nameof(ReducerMethodAttribute.ActionType)} specified. "
						+ methodInfo.GetClassNameAndMethodName(),
					nameof(MethodInfo));

			if (attribute.ActionType != null && methodParameters.Length != 1)
				throw new ArgumentException(
					$"Method must have 1 parameter (state)"
						+ $" when [{nameof(ReducerMethodAttribute)}] has an {nameof(ReducerMethodAttribute.ActionType)} specified. "
						+ methodInfo.GetClassNameAndMethodName(),
					nameof(methodInfo));

			if (methodInfo.ReturnType != methodParameters[0].ParameterType)
				throw new ArgumentException(
					$"Expected reducer method to return type {methodInfo.ReturnType.FullName}. " + methodInfo.GetClassNameAndMethodName(),
					nameof(methodInfo));

			HostClassType = hostClassType;
			MethodInfo = methodInfo;
			StateType = methodParameters[0].ParameterType;
			ActionType = attribute.ActionType ?? methodParameters[1].ParameterType;
			RequiresActionParameterInMethod = attribute.ActionType == null;
		}
	}
}
