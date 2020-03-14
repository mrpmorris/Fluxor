using System;

namespace Fluxor.DependencyInjection
{
	internal class DiscoveredEffectClass
	{
		public readonly Type ImplementingType;

		public DiscoveredEffectClass(Type implementingType)
		{
			ImplementingType = implementingType;
		}
	}
}
