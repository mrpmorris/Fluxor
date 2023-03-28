using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Fluxor.StoreBuilderSourceGenerator.ModelSelectors;

public static class ClassesSelector
{
	public static IncrementalValuesProvider<GeneratorSyntaxContext> Select(IncrementalGeneratorInitializationContext context) =>
		context.SyntaxProvider.CreateSyntaxProvider(
			predicate: static (syntaxNode, _) => syntaxNode is TypeDeclarationSyntax typeDeclaration && typeDeclaration.Kind() == SyntaxKind.ClassDeclaration,
			transform: static (syntaxContext, _) => syntaxContext);
}
