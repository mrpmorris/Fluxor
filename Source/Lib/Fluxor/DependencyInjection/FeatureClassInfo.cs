using Fluxor.DependencyInjection.Generated;
using System;

namespace Fluxor.DependencyInjection;

internal class FeatureClassInfo
{
	public readonly Type FeatureInterfaceGenericType;
	public readonly Type ImplementingType;
	public readonly Type StateType;
	// Set when the feature is described by generated code
	public readonly Action<IFeature, object, int> AddReducer;
	public readonly int GeneratedStateIndex;

	public FeatureClassInfo(Type implementingType, Type stateType)
	{
		FeatureInterfaceGenericType = typeof(IFeature<>).MakeGenericType(stateType);
		ImplementingType = implementingType;
		StateType = stateType;
	}

	public FeatureClassInfo(Type implementingType, GeneratedFeatureClass generatedFeatureClass)
	{
		FeatureInterfaceGenericType = generatedFeatureClass.FeatureInterfaceType;
		ImplementingType = implementingType;
		StateType = generatedFeatureClass.StateType;
		AddReducer = generatedFeatureClass.AddReducer;
		GeneratedStateIndex = generatedFeatureClass.StateIndex;
	}
}
