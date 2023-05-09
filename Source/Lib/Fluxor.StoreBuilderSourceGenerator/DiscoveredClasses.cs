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
		ImmutableArray<string> GeneratedEffectClassNames = default,
		ImmutableArray<string> GeneratedFeatureClassNames = default,
		ImmutableArray<string> GeneratedReducerClassNames = default,
		ImmutableArray<string> GeneratedEffectDependenciesClassNames = default
	);
}

