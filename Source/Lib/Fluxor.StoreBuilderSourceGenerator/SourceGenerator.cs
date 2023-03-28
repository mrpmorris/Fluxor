using Fluxor.StoreBuilderSourceGenerator.ModelSelectors;
using Microsoft.CodeAnalysis;
using System;

namespace Fluxor.StoreBuilderSourceGenerator;

[Generator]
public class SourceGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		
		IncrementalValuesProvider<GeneratorSyntaxContext> classDeclarationSyntaxes =
			ClassesSelector.Select(context);

		context.RegisterSourceOutput(
			classDeclarationSyntaxes,
			static (sourceProductionContext, generatorSyntaxContext) =>
			{
				sourceProductionContext.AddSource(
					hintName: $"Hello.g.cs",
					source: "//haha");
			});
	}
}
