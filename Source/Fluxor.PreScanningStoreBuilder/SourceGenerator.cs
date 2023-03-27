using Fluxor.PreScanningStoreBuilder.SourceGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Fluxor.PreScanningStoreBuilder
{
	[Generator]
	public class SourceGenerator : IIncrementalGenerator
	{
		public void Initialize(IncrementalGeneratorInitializationContext context)
		{
			//IncrementalValuesProvider<GeneratorSyntaxContext> classDeclarationSyntaxes = SelectTypeDeclarationSyntaxes(context);
			//FeatureStateSourceBuilder.Build(context, classDeclarationSyntaxes);
		}

		//private static IncrementalValuesProvider<GeneratorSyntaxContext> SelectTypeDeclarationSyntaxes(IncrementalGeneratorInitializationContext context) =>
		//	context.SyntaxProvider.CreateSyntaxProvider(
		//		predicate: static (syntaxNode, _) => syntaxNode is TypeDeclarationSyntax typeDeclaration && typeDeclaration.Kind() == SyntaxKind.ClassDeclaration,
		//		transform: static (syntaxContext, _) => syntaxContext);
	}
}
