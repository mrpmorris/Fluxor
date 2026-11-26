using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Fluxor.DependencyInjection.Generated;

/// <summary>
/// Builds the <see cref="GeneratedTypeEntry"/> descriptors of an assembly from a compact table, so generated code
/// needs little code (and few, small methods) to describe many types. Intended for generated code only.
/// </summary>
/// <remarks>
/// The table is a sequence of entries, read to its end. Each entry is, in this order:
/// <list type="number">
///   <item>the index in <c>types</c> of the described type;</item>
///   <item>flags: <see cref="IsMiddleware"/>, <see cref="IsEffectClass"/>, <see cref="HasFeatureClass"/>,
///   <see cref="HasFeatureState"/>, <see cref="HasReducerClass"/>;</item>
///   <item>with <see cref="HasFeatureClass"/>: the indexes in <c>types</c> of the state type and of the closed
///   IFeature of TState type, then the state index;</item>
///   <item>with <see cref="HasFeatureState"/>: the same three values;</item>
///   <item>with <see cref="HasReducerClass"/>: the index in <c>types</c> of the reduced state type;</item>
///   <item>the number of reducer methods; when not 0, the index in <c>reducerFactories</c> of their factory, then
///   for each reducer method <c>(index in types of its state type) * 2 + (1 if static, else 0)</c>; the reducer
///   method's <see cref="GeneratedReducerMethod.Index"/> is its position in the list;</item>
///   <item>the number of effect methods; when not 0, the index in <c>effectFactories</c> of their factory, then
///   for each effect method 1 if static, else 0; the effect method's <see cref="GeneratedEffectMethod.Index"/> is
///   its position in the list.</item>
/// </list>
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class GeneratedRegistrationTable
{
	/// <summary>Entry flag: the type is a middleware.</summary>
	public const int IsMiddleware = 1;

	/// <summary>Entry flag: the type is an effect class.</summary>
	public const int IsEffectClass = 2;

	/// <summary>Entry flag: the type is a feature class.</summary>
	public const int HasFeatureClass = 4;

	/// <summary>Entry flag: the type is decorated with FeatureState.</summary>
	public const int HasFeatureState = 8;

	/// <summary>Entry flag: the type is a reducer class.</summary>
	public const int HasReducerClass = 16;

	/// <summary>
	/// Creates the descriptors the table describes.
	/// </summary>
	/// <param name="types">The types the table refers to by index</param>
	/// <param name="reducerFactories">The reducer factories the table refers to by index</param>
	/// <param name="effectFactories">The effect factories the table refers to by index</param>
	/// <param name="createFeature">Creates the feature of a FeatureState, given its state index (null when there is none)</param>
	/// <param name="addReducer">Adds a reducer to a feature, given its state index (null when there is no feature)</param>
	/// <param name="table">The table (see the remarks on <see cref="GeneratedRegistrationTable"/>)</param>
	public static GeneratedTypeEntry[] CreateEntries(
		Type[] types,
		Func<object, int, object>[] reducerFactories,
		Func<object, int, IEffect>[] effectFactories,
		Func<int, IFeature> createFeature,
		Action<IFeature, object, int> addReducer,
		int[] table)
	{
		if (table is null)
			throw new ArgumentNullException(nameof(table));

		var entries = new List<GeneratedTypeEntry>();
		int position = 0;
		int Next() => table[position++];
		while (position < table.Length)
		{
			Type type = types[Next()];
			int flags = Next();

			GeneratedFeatureClass featureClass = (flags & HasFeatureClass) == 0
				? null
				: new GeneratedFeatureClass(types[Next()], types[Next()], addReducer, Next());

			GeneratedFeatureState featureState = (flags & HasFeatureState) == 0
				? null
				: new GeneratedFeatureState(types[Next()], types[Next()], createFeature, addReducer, Next());

			Type reducerClassStateType = (flags & HasReducerClass) == 0 ? null : types[Next()];

			GeneratedReducerMethod[] reducerMethods = null;
			int reducerCount = Next();
			if (reducerCount > 0)
			{
				reducerMethods = new GeneratedReducerMethod[reducerCount];
				Func<object, int, object> factory = reducerFactories[Next()];
				for (int index = 0; index < reducerCount; index++)
				{
					int stateTypeAndIsStatic = Next();
					reducerMethods[index] = new GeneratedReducerMethod(
						types[stateTypeAndIsStatic >> 1],
						(stateTypeAndIsStatic & 1) == 1,
						factory,
						index);
				}
			}

			GeneratedEffectMethod[] effectMethods = null;
			int effectCount = Next();
			if (effectCount > 0)
			{
				effectMethods = new GeneratedEffectMethod[effectCount];
				Func<object, int, IEffect> factory = effectFactories[Next()];
				for (int index = 0; index < effectCount; index++)
					effectMethods[index] = new GeneratedEffectMethod(Next() == 1, factory, index);
			}

			entries.Add(new GeneratedTypeEntry(
				type,
				isMiddleware: (flags & IsMiddleware) != 0,
				featureClass: featureClass,
				featureState: featureState,
				reducerClassStateType: reducerClassStateType,
				isEffectClass: (flags & IsEffectClass) != 0,
				reducerMethods: reducerMethods,
				effectMethods: effectMethods));
		}

		return entries.ToArray();
	}
}
