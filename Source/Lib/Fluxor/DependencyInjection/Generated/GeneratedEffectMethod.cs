using System;
using System.ComponentModel;

namespace Fluxor.DependencyInjection.Generated;

/// <summary>
/// Describes a method decorated with EffectMethod. Intended for generated code only.
/// </summary>
/// <remarks>
/// The action type is not described: the effect created by <see cref="CreateEffect"/> knows it.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class GeneratedEffectMethod
{
	/// <summary>True if the method is static.</summary>
	public bool IsStatic { get; }

	/// <summary>
	/// Creates the effect. The first argument is the host instance (or null for static methods), the second is
	/// <see cref="Index"/>. Generated code shares one factory between the effect methods of a host and selects
	/// the method by the index.
	/// </summary>
	public Func<object, int, IEffect> CreateEffect { get; }

	/// <summary>The value passed to <see cref="CreateEffect"/> to create this effect.</summary>
	public int Index { get; }

	/// <summary>
	/// Creates a new instance.
	/// </summary>
	/// <param name="isStatic">True if the method is static</param>
	/// <param name="createEffect">Creates the effect from the host instance (or null) and <paramref name="index"/></param>
	/// <param name="index">The value passed to <paramref name="createEffect"/> to create this effect</param>
	public GeneratedEffectMethod(bool isStatic, Func<object, int, IEffect> createEffect, int index)
	{
		IsStatic = isStatic;
		CreateEffect = createEffect;
		Index = index;
	}
}
