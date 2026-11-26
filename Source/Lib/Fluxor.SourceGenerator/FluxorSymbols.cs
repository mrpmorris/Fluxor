using Microsoft.CodeAnalysis;

namespace Fluxor.SourceGenerator;

/// <summary>
/// The Fluxor types the generator looks for, resolved by metadata name from the compilation's references
/// (the generator cannot reference Fluxor itself).
/// </summary>
internal sealed class FluxorSymbols
{
	public const string GeneratedRegistrationAttributeName =
		"Fluxor.DependencyInjection.Generated.FluxorGeneratedRegistrationAttribute";

	public INamedTypeSymbol FeatureInterface { get; }
	public INamedTypeSymbol ReducerInterface { get; }
	public INamedTypeSymbol EffectInterface { get; }
	public INamedTypeSymbol MiddlewareInterface { get; }
	public INamedTypeSymbol Dispatcher { get; }
	public INamedTypeSymbol FeatureStateAttribute { get; }
	public INamedTypeSymbol ReducerMethodAttribute { get; }
	public INamedTypeSymbol EffectMethodAttribute { get; }
	public INamedTypeSymbol Task { get; }
	public INamedTypeSymbol? ObsoleteAttribute { get; }
	public INamedTypeSymbol? ExperimentalAttribute { get; }

	private FluxorSymbols(
		INamedTypeSymbol featureInterface,
		INamedTypeSymbol reducerInterface,
		INamedTypeSymbol effectInterface,
		INamedTypeSymbol middlewareInterface,
		INamedTypeSymbol dispatcher,
		INamedTypeSymbol featureStateAttribute,
		INamedTypeSymbol reducerMethodAttribute,
		INamedTypeSymbol effectMethodAttribute,
		INamedTypeSymbol task,
		INamedTypeSymbol? obsoleteAttribute,
		INamedTypeSymbol? experimentalAttribute)
	{
		FeatureInterface = featureInterface;
		ReducerInterface = reducerInterface;
		EffectInterface = effectInterface;
		MiddlewareInterface = middlewareInterface;
		Dispatcher = dispatcher;
		FeatureStateAttribute = featureStateAttribute;
		ReducerMethodAttribute = reducerMethodAttribute;
		EffectMethodAttribute = effectMethodAttribute;
		Task = task;
		ObsoleteAttribute = obsoleteAttribute;
		ExperimentalAttribute = experimentalAttribute;
	}

	/// <summary>
	/// True when the compilation references Fluxor at all (otherwise there is nothing to register).
	/// </summary>
	public static bool ReferencesFluxor(Compilation compilation) =>
		compilation.GetTypeByMetadataName("Fluxor.IFeature") is not null;

	/// <summary>
	/// Resolves the symbols, or returns null when the referenced Fluxor lacks any of them
	/// (for example a Fluxor version without generated registration support).
	/// </summary>
	public static FluxorSymbols? TryCreate(Compilation compilation)
	{
		INamedTypeSymbol? Get(string name) => compilation.GetTypeByMetadataName(name);

		INamedTypeSymbol? featureInterface = Get("Fluxor.IFeature`1");
		INamedTypeSymbol? reducerInterface = Get("Fluxor.IReducer`1");
		INamedTypeSymbol? effectInterface = Get("Fluxor.IEffect");
		INamedTypeSymbol? middlewareInterface = Get("Fluxor.IMiddleware");
		INamedTypeSymbol? dispatcher = Get("Fluxor.IDispatcher");
		INamedTypeSymbol? featureStateAttribute = Get("Fluxor.FeatureStateAttribute");
		INamedTypeSymbol? reducerMethodAttribute = Get("Fluxor.ReducerMethodAttribute");
		INamedTypeSymbol? effectMethodAttribute = Get("Fluxor.EffectMethodAttribute");
		INamedTypeSymbol? task = Get("System.Threading.Tasks.Task");

		if (featureInterface is null
			|| reducerInterface is null
			|| effectInterface is null
			|| middlewareInterface is null
			|| dispatcher is null
			|| featureStateAttribute is null
			|| reducerMethodAttribute is null
			|| effectMethodAttribute is null
			|| task is null
			|| Get(GeneratedRegistrationAttributeName) is null
			|| Get("Fluxor.DependencyInjection.Generated.IFluxorGeneratedRegistration") is null
			|| Get("Fluxor.DependencyInjection.Generated.GeneratedWrappers") is null
			|| Get("Fluxor.DependencyInjection.Generated.GeneratedRegistrationTable") is null)
			return null;

		return new FluxorSymbols(
			featureInterface,
			reducerInterface,
			effectInterface,
			middlewareInterface,
			dispatcher,
			featureStateAttribute,
			reducerMethodAttribute,
			effectMethodAttribute,
			task,
			Get("System.ObsoleteAttribute"),
			Get("System.Diagnostics.CodeAnalysis.ExperimentalAttribute"));
	}

	/// <summary>
	/// The type arguments of every implemented closed form of the generic interface definition.
	/// </summary>
	public static IEnumerable<ITypeSymbol> GetImplementedTypeArguments(INamedTypeSymbol type, INamedTypeSymbol genericInterface) =>
		type.AllInterfaces
			.Where(x => SymbolEqualityComparer.Default.Equals(x.OriginalDefinition, genericInterface))
			.Select(x => x.TypeArguments[0]);

	public static bool Implements(INamedTypeSymbol type, INamedTypeSymbol @interface) =>
		type.AllInterfaces.Any(x => SymbolEqualityComparer.Default.Equals(x, @interface));

	/// <summary>
	/// True when <paramref name="type"/> is <paramref name="baseType"/> or derives from it.
	/// </summary>
	public static bool IsOrDerivesFrom(INamedTypeSymbol? type, INamedTypeSymbol baseType)
	{
		for (INamedTypeSymbol? current = type; current is not null; current = current.BaseType)
			if (SymbolEqualityComparer.Default.Equals(current, baseType))
				return true;
		return false;
	}
}
