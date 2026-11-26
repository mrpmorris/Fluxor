using Fluxor.DependencyInjection.Generated;
using Fluxor.Extensions;
using System;
using System.Reflection;

namespace Fluxor.DependencyInjection;

internal class ReducerMethodInfo
{
	public readonly Type HostClassType;
	public readonly MethodInfo MethodInfo;
	public readonly Type StateType;
	public readonly Type ActionType;
	public readonly bool RequiresActionParameterInMethod;
	public readonly bool IsStatic;
	// Set when the method is described by generated code; MethodInfo and ActionType are then null
	public readonly Func<object, int, object> CreateReducer;
	public readonly int GeneratedIndex;

	public ReducerMethodInfo(Type hostClassType, GeneratedReducerMethod generatedReducerMethod)
	{
		HostClassType = hostClassType;
		StateType = generatedReducerMethod.StateType;
		IsStatic = generatedReducerMethod.IsStatic;
		CreateReducer = generatedReducerMethod.CreateReducer;
		GeneratedIndex = generatedReducerMethod.Index;
	}

	public ReducerMethodInfo(Type hostClassType, ReducerMethodAttribute attribute, MethodInfo methodInfo)
	{
		ParameterInfo[] methodParameters = methodInfo.GetParameters();
		if (attribute.ActionType is null && methodParameters.Length != 2)
			throw new ArgumentException(
				$"Method must have 2 parameters (state, action)"
					+ $" when [{nameof(ReducerMethodAttribute)}] has no {nameof(ReducerMethodAttribute.ActionType)} specified. "
					+ methodInfo.GetClassNameAndMethodName(),
				nameof(MethodInfo));

		if (attribute.ActionType is not null && methodParameters.Length != 1)
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
		RequiresActionParameterInMethod = attribute.ActionType is null;
		IsStatic = methodInfo.IsStatic;
	}
}
