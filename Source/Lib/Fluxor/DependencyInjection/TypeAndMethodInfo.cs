using Fluxor.DependencyInjection.Generated;
using System;
using System.Reflection;

namespace Fluxor.DependencyInjection;

internal struct TypeAndMethodInfo
{
	public readonly Type Type;
	public readonly MethodInfo MethodInfo;
	public readonly GeneratedReducerMethod GeneratedReducerMethod;
	public readonly GeneratedEffectMethod GeneratedEffectMethod;

	public TypeAndMethodInfo(Type type, MethodInfo methodInfo)
	{
		Type = type ?? throw new ArgumentNullException(nameof(type));
		MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
		GeneratedReducerMethod = null;
		GeneratedEffectMethod = null;
	}

	public TypeAndMethodInfo(Type type, GeneratedReducerMethod generatedReducerMethod)
	{
		Type = type ?? throw new ArgumentNullException(nameof(type));
		MethodInfo = null;
		GeneratedReducerMethod = generatedReducerMethod ?? throw new ArgumentNullException(nameof(generatedReducerMethod));
		GeneratedEffectMethod = null;
	}

	public TypeAndMethodInfo(Type type, GeneratedEffectMethod generatedEffectMethod)
	{
		Type = type ?? throw new ArgumentNullException(nameof(type));
		MethodInfo = null;
		GeneratedReducerMethod = null;
		GeneratedEffectMethod = generatedEffectMethod ?? throw new ArgumentNullException(nameof(generatedEffectMethod));
	}
}
