using Fluxor.StoreBuilderSourceGenerator.FeatureStateClasses;
using Fluxor.StoreBuilderSourceGenerator.ReducerMethodClasses;
using Microsoft.CodeAnalysis;
using System;

namespace Fluxor.StoreBuilderSourceGenerator;

[Generator]
public class SourceGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{

		IncrementalValuesProvider< FeatureStateClassInfo> featureStateClassInfos =
			FeatureStateClassesSelector.Select(context);

		IncrementalValuesProvider<Either<CompilerError, ReducerMethodInfo>> reducerMethodInfos =
			ReducerMethodsSelector.Select(context);

		context.RegisterSourceOutput(
			featureStateClassInfos,
			static (productionContext, sourceContext) =>
			{
				Console.Beep(11000, 150);
			});

		context.RegisterSourceOutput(
			reducerMethodInfos,
			static (productionContext, sourceContext) =>
			{
				_ = sourceContext.Match(
					error =>
					{
						var descriptor = new DiagnosticDescriptor(
							id: error.Id,
							title: error.Title,
							messageFormat: error.Title,
							category: "Fluxor",
							defaultSeverity: DiagnosticSeverity.Error,
							isEnabledByDefault: true);
						var diagnostic = Diagnostic.Create(descriptor, error.Location);
						productionContext.ReportDiagnostic(diagnostic);
						return Void.Value;
					},
					reducerMethodInfo =>
					{
						return Void.Value;
					});
				Console.Beep(7000, 150);
			});
	}

	static SourceGenerator()
	{

	}
}


