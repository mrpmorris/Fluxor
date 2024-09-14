using System;

namespace Fluxor.DependencyInjection;

internal class EffectClassInfo
{
	public readonly Type ImplementingType;

	public EffectClassInfo(Type implementingType)
	{
		ImplementingType = implementingType;
	}
}
