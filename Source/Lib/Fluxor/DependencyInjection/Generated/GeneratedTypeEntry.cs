using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Fluxor.DependencyInjection.Generated;

/// <summary>
/// Describes how a single type participates in Fluxor registration. Intended for generated code only.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class GeneratedTypeEntry
{
	/// <summary>The described type.</summary>
	public Type Type { get; }

	/// <summary>True if the type is a middleware.</summary>
	public bool IsMiddleware { get; }

	/// <summary>Set if the type is a feature class.</summary>
	public GeneratedFeatureClass FeatureClass { get; }

	/// <summary>Set if the type is a feature state.</summary>
	public GeneratedFeatureState FeatureState { get; }

	/// <summary>Set if the type is a reducer class; the state type it reduces.</summary>
	public Type ReducerClassStateType { get; }

	/// <summary>True if the type is an effect class.</summary>
	public bool IsEffectClass { get; }

	/// <summary>Reducer methods declared on the type. Never null.</summary>
	public IReadOnlyList<GeneratedReducerMethod> ReducerMethods { get; }

	/// <summary>Effect methods declared on the type. Never null.</summary>
	public IReadOnlyList<GeneratedEffectMethod> EffectMethods { get; }

	/// <summary>
	/// Creates a new instance.
	/// </summary>
	/// <param name="type">The described type</param>
	/// <param name="isMiddleware">True if the type is a middleware</param>
	/// <param name="featureClass">Feature class information, if any</param>
	/// <param name="featureState">Feature state information, if any</param>
	/// <param name="reducerClassStateType">State type of a reducer class, if any</param>
	/// <param name="isEffectClass">True if the type is an effect class</param>
	/// <param name="reducerMethods">Reducer methods, if any</param>
	/// <param name="effectMethods">Effect methods, if any</param>
	public GeneratedTypeEntry(
		Type type,
		bool isMiddleware = false,
		GeneratedFeatureClass featureClass = null,
		GeneratedFeatureState featureState = null,
		Type reducerClassStateType = null,
		bool isEffectClass = false,
		IReadOnlyList<GeneratedReducerMethod> reducerMethods = null,
		IReadOnlyList<GeneratedEffectMethod> effectMethods = null)
	{
		Type = type;
		IsMiddleware = isMiddleware;
		FeatureClass = featureClass;
		FeatureState = featureState;
		ReducerClassStateType = reducerClassStateType;
		IsEffectClass = isEffectClass;
		ReducerMethods = reducerMethods ?? Array.Empty<GeneratedReducerMethod>();
		EffectMethods = effectMethods ?? Array.Empty<GeneratedEffectMethod>();
	}
}
