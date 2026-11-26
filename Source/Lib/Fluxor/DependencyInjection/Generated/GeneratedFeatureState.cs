using System;
using System.ComponentModel;

namespace Fluxor.DependencyInjection.Generated;

/// <summary>
/// Describes a state type decorated with FeatureState. Intended for generated code only.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class GeneratedFeatureState
{
	/// <summary>The state type.</summary>
	public Type StateType { get; }

	/// <summary>The closed IFeature of TState interface type.</summary>
	public Type FeatureInterfaceType { get; }

	/// <summary>
	/// Creates the feature for the state. The argument is <see cref="StateIndex"/>: generated code may share one
	/// method between state types and select the state type by the index.
	/// </summary>
	public Func<int, IFeature> CreateFeature { get; }

	/// <summary>
	/// Adds a reducer (an IReducer of TState, boxed) to the given feature. The third argument is
	/// <see cref="StateIndex"/>.
	/// </summary>
	public Action<IFeature, object, int> AddReducer { get; }

	/// <summary>The value passed to <see cref="CreateFeature"/> and <see cref="AddReducer"/>.</summary>
	public int StateIndex { get; }

	/// <summary>
	/// Creates a new instance.
	/// </summary>
	/// <param name="stateType">The state type</param>
	/// <param name="featureInterfaceType">The closed IFeature of TState interface type</param>
	/// <param name="createFeature">Creates the feature, given <paramref name="stateIndex"/></param>
	/// <param name="addReducer">Adds a reducer to a feature, given <paramref name="stateIndex"/></param>
	/// <param name="stateIndex">The value passed to <paramref name="createFeature"/> and <paramref name="addReducer"/></param>
	public GeneratedFeatureState(
		Type stateType,
		Type featureInterfaceType,
		Func<int, IFeature> createFeature,
		Action<IFeature, object, int> addReducer,
		int stateIndex)
	{
		StateType = stateType;
		FeatureInterfaceType = featureInterfaceType;
		CreateFeature = createFeature;
		AddReducer = addReducer;
		StateIndex = stateIndex;
	}
}
