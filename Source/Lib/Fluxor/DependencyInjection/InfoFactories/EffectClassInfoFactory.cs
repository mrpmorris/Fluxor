using Fluxor.DependencyInjection.Generated;
using Fluxor.DependencyInjection.Wrappers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluxor.DependencyInjection.InfoFactories;

internal static class EffectClassInfoFactory
{
	internal static EffectClassInfo[] Create(
		IServiceCollection services,
		IEnumerable<Type> allCandidateTypes,
		GeneratedRegistrationCatalog catalog)
	=>
		allCandidateTypes
			.Where(t =>
				catalog.TryGetEntry(t, out GeneratedTypeEntry entry)
				? entry.IsEffectClass
				: typeof(IEffect).IsAssignableFrom(t))
			.Where(t => t != typeof(EffectWrapper<>))
			.Select(t => new EffectClassInfo(implementingType: t))
			.ToArray();
}
