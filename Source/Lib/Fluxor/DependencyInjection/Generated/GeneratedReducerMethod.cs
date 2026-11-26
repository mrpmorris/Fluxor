using System;
using System.ComponentModel;

namespace Fluxor.DependencyInjection.Generated;

/// <summary>
/// Describes a method decorated with ReducerMethod. Intended for generated code only.
/// </summary>
/// <remarks>
/// The action type is not described: the reducer created by <see cref="CreateReducer"/> knows it.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class GeneratedReducerMethod
{
	/// <summary>The state type being reduced.</summary>
	public Type StateType { get; }

	/// <summary>True if the method is static.</summary>
	public bool IsStatic { get; }

	/// <summary>
	/// Creates the reducer. The first argument is the host instance (or null for static methods), the second is
	/// <see cref="Index"/>. Generated code shares one factory between the reducer methods of a host and selects
	/// the method by the index.
	/// </summary>
	public Func<object, int, object> CreateReducer { get; }

	/// <summary>The value passed to <see cref="CreateReducer"/> to create this reducer.</summary>
	public int Index { get; }

	/// <summary>
	/// Creates a new instance.
	/// </summary>
	/// <param name="stateType">The state type being reduced</param>
	/// <param name="isStatic">True if the method is static</param>
	/// <param name="createReducer">Creates the reducer from the host instance (or null) and <paramref name="index"/></param>
	/// <param name="index">The value passed to <paramref name="createReducer"/> to create this reducer</param>
	public GeneratedReducerMethod(
		Type stateType,
		bool isStatic,
		Func<object, int, object> createReducer,
		int index)
	{
		StateType = stateType;
		IsStatic = isStatic;
		CreateReducer = createReducer;
		Index = index;
	}
}
