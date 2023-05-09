using Fluxor.StoreBuilderSourceGenerator.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Fluxor.StoreBuilderSourceGenerator.DiscoveredMiddlewareClasses;

internal static class ReducerClassesSelector
{
	public static IncrementalValuesProvider<string> Select(IncrementalGeneratorInitializationContext context)
	{
		IncrementalValueProvider<INamedTypeSymbol> fluxorIMiddlewareTypeProvider = context.CompilationProvider.Select(
			static (Compilation compilation, CancellationToken cancellationToken) =>
				compilation.GetTypeByMetadataName("Fluxor.IReducer`1"));

		var classSymbols = context
			.SyntaxProvider
			.CreateSyntaxProvider
			(
				predicate: static (SyntaxNode node, CancellationToken _) => node.IsKind(SyntaxKind.ClassDeclaration),
				transform: static (x, cancellationToken) => (INamedTypeSymbol)x.SemanticModel.GetDeclaredSymbol(x.Node, cancellationToken)
			);

		IncrementalValuesProvider<string> provider = classSymbols
			.Combine(fluxorIMiddlewareTypeProvider)
			.Where(static x => ImplementsGenericInterface(x.Left.AllInterfaces, x.Right))
			.Select(static (x, cancellationToken) => NamespaceHelper.Combine(x.Left.ContainingNamespace.ToDisplayString(), x.Left.Name));

		return provider;
	}

	private static bool ImplementsGenericInterface(ImmutableArray<INamedTypeSymbol> allInterfaces, INamedTypeSymbol interfaceSymbol)
	{
		for (int i = 0; i < allInterfaces.Length; i++)
		{
			var implementedInterface = allInterfaces[i];
			if (implementedInterface.OriginalDefinition.Equals(interfaceSymbol, SymbolEqualityComparer.Default))
				return true;
		}

		return false;
	}
}
