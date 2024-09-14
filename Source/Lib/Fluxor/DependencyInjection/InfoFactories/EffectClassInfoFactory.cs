using Fluxor.DependencyInjection.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluxor.DependencyInjection.InfoFactories;

internal static class EffectClassInfoFactory
{
	internal static EffectClassInfo[] Create(IEnumerable<Type> allCandidateTypes) =>
		allCandidateTypes
			.Where(t => typeof(IEffect).IsAssignableFrom(t))
			.Where(t => t != typeof(EffectWrapper<>))
			.Select(t => new EffectClassInfo(implementingType: t))
			.ToArray();
}
