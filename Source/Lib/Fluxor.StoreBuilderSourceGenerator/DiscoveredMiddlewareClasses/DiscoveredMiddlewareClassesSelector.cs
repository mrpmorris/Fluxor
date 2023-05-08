using Fluxor.StoreBuilderSourceGenerator.Extensions;
using Fluxor.StoreBuilderSourceGenerator.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Fluxor.StoreBuilderSourceGenerator.DiscoveredMiddlewareClasses;

internal static class DiscoveredMiddlewareClassesSelector
{
	public static IncrementalValuesProvider<DiscoveredMiddlewareClassInfo> Select(IncrementalGeneratorInitializationContext context)
	{
		IncrementalValueProvider<INamedTypeSymbol> fluxorIMiddlewareTypeProvider = context.CompilationProvider.Select(
			static (Compilation compilation, CancellationToken cancellationToken) =>
				compilation.GetTypeByMetadataName("Fluxor.IMiddleware"));



		var syntaxProvider = context
			.SyntaxProvider
			.CreateSyntaxProvider
			(
				predicate: static (SyntaxNode node, CancellationToken _) => node.IsKind(SyntaxKind.ClassDeclaration),
				transform: (x, cancellationToken) => (INamedTypeSymbol)x.SemanticModel.GetDeclaredSymbol(x.Node, cancellationToken)
			);

		var provider = syntaxProvider.Combine(fluxorIMiddlewareTypeProvider)
			.Where(x => x.Left.AllInterfaces.Contains(x.Right))
			.Select(static (x, cancellationToken) =>
				new DiscoveredMiddlewareClassInfo(
					ClassName: x.Left.Name,
					ClassNamespace: x.Left.ContainingNamespace.ToDisplayString()));

		return provider;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static DiscoveredMiddlewareClassInfo CreateMiddlewareClassInfo(GeneratorSyntaxContext context, CancellationToken cancellationToken)
	{
		var middlewareSymbol = context.SemanticModel.GetDeclaredSymbol(context.Node);
		return DiscoveredMiddlewareClassInfo.None;
	}
}
