using System;
using System.ComponentModel;

namespace Fluxor.DependencyInjection.Generated;

/// <summary>
/// Describes a feature class (a type deriving from Feature of TState). Intended for generated code only.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class GeneratedFeatureClass
{
	/// <summary>The state type of the feature.</summary>
	public Type StateType { get; }

	/// <summary>The closed IFeature of TState interface type.</summary>
	public Type FeatureInterfaceType { get; }

	/// <summary>
	/// Adds a reducer (an IReducer of TState, boxed) to the given feature. The third argument is
	/// <see cref="StateIndex"/>: generated code may share one method between state types and select the state
	/// type by the index.
	/// </summary>
	public Action<IFeature, object, int> AddReducer { get; }

	/// <summary>The value passed to <see cref="AddReducer"/> as its third argument.</summary>
	public int StateIndex { get; }

	/// <summary>
	/// Creates a new instance.
	/// </summary>
	/// <param name="stateType">The state type of the feature</param>
	/// <param name="featureInterfaceType">The closed IFeature of TState interface type</param>
	/// <param name="addReducer">Adds a reducer to a feature, given <paramref name="stateIndex"/></param>
	/// <param name="stateIndex">The value passed to <paramref name="addReducer"/> as its third argument</param>
	public GeneratedFeatureClass(
		Type stateType,
		Type featureInterfaceType,
		Action<IFeature, object, int> addReducer,
		int stateIndex)
	{
		StateType = stateType;
		FeatureInterfaceType = featureInterfaceType;
		AddReducer = addReducer;
		StateIndex = stateIndex;
	}
}
