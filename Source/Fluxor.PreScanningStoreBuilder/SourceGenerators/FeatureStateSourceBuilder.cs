using Fluxor.PreScanningStoreBuilder.CodeModels;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace Fluxor.PreScanningStoreBuilder.SourceGenerators
{
	internal static class FeatureStateSourceBuilder
	{
		internal static void Build(
			IncrementalGeneratorInitializationContext context,
			IncrementalValuesProvider<GeneratorSyntaxContext> classDeclarationSyntaxContexts)
		{
			//var source = classDeclarationSyntaxContexts
			//	.Select(static (syntaxContext, cancellationToken) => new ClassInfoWithAttributeInfo(syntaxContext, "Fluxor.FeatureStateAttribute"))
			//	.Where(x => !x.IsEmpty);

			//context.RegisterSourceOutput(
			//	source,
			//	static (productionContext, stateClassInfo) =>
			//	{
			//		//productionContext.AddSource(
			//		//	hintName: $"{stateClassInfo.ClassInfo.FullName}.Fluxor.g.cs",
			//		//	source: $"namespace {stateClassInfo.ClassInfo.Namespace};\r\npublic class {stateClassInfo.ClassInfo.Name}Extensions {{ public static void X() {{}} }} ");
			//	});
		}
	}
}
