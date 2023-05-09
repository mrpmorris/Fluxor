using Fluxor.StoreBuilderSourceGenerator.EffectMethodAttributes;
using Fluxor.StoreBuilderSourceGenerator.FeatureStateAttributes;
using Fluxor.StoreBuilderSourceGenerator.ReducerMethodAttributes;
using System.Collections.Immutable;

namespace Fluxor.StoreBuilderSourceGenerator;

public partial class SourceGenerator
{
	private readonly record struct DiscoveredClasses
	(
		ImmutableArray<string> DiscoveredEffectClassNames = default,
		ImmutableArray<string> DiscoveredFeatureClassNames = default,
		ImmutableArray<string> DiscoveredMiddlewareClassNames = default,
		ImmutableArray<string> DiscoveredReducerClassNames = default,
		ImmutableArray<EffectMethodInfo> EffectMethodInfos = default,
		ImmutableArray<FeatureStateClassInfo> FeatureStateClassInfos = default,
		ImmutableArray<ReducerMethodInfo> ReducerMethodInfos = default
	);
}

