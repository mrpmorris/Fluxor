using Fluxor.StoreBuilderSourceGenerator.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using System.Threading;

namespace Fluxor.StoreBuilderSourceGenerator.DiscoveredMiddlewareClasses;

internal static class FeatureClassesSelector
{
	public static IncrementalValuesProvider<string> Select(IncrementalGeneratorInitializationContext context)
	{
		IncrementalValueProvider<INamedTypeSymbol> fluxorIMiddlewareTypeProvider = context.CompilationProvider.Select(
			static (Compilation compilation, CancellationToken cancellationToken) =>
				compilation.GetTypeByMetadataName("Fluxor.IFeature"));

		var classSymbols = context
			.SyntaxProvider
			.CreateSyntaxProvider
			(
				predicate: static (SyntaxNode node, CancellationToken _) => node.IsKind(SyntaxKind.ClassDeclaration),
				transform: static (x, cancellationToken) => (INamedTypeSymbol)x.SemanticModel.GetDeclaredSymbol(x.Node, cancellationToken)
			);

		IncrementalValuesProvider<string> provider = classSymbols
			.Combine(fluxorIMiddlewareTypeProvider)
			.Where(static x => x.Left.AllInterfaces.Contains(x.Right))
			.Select(static (x, cancellationToken) => NamespaceHelper.Combine(x.Left.ContainingNamespace.ToDisplayString(), x.Left.Name));

		return provider;
	}
}
