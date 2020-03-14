using System;

namespace Fluxor.DependencyInjection
{
	internal class DiscoveredFeatureClass
	{
		public readonly Type FeatureInterfaceGenericType;
		public readonly Type ImplementingType;
		public readonly Type StateType;

		public DiscoveredFeatureClass(Type implementingType, Type stateType)
		{
			FeatureInterfaceGenericType = typeof(IFeature<>).MakeGenericType(stateType);
			ImplementingType = implementingType;
			StateType = stateType;
		}
	}
}
