using Fluxor.StoreBuilderSourceGenerator.EffectMethodAttributes;
using Fluxor.StoreBuilderSourceGenerator.FeatureStateAttributes;
using Fluxor.StoreBuilderSourceGenerator.ReducerMethodAttributes;
using Microsoft.CodeAnalysis;
using System;

namespace Fluxor.StoreBuilderSourceGenerator;

[Generator]
public class SourceGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		IncrementalValuesProvider<Either<CompilerError, FeatureStateClassInfo>> featureStateClassInfos =
			FeatureStateClassesSelector.Select(context);

		IncrementalValuesProvider<Either<CompilerError, ReducerMethodInfo>> reducerMethodInfos =
			ReducerMethodsSelector.Select(context);

		IncrementalValuesProvider<Either<CompilerError, EffectMethodInfo>> effectMethodInfos =
			EffectMethodsSelector.Select(context);

		context.RegisterSourceOutput(
			featureStateClassInfos,
			static (productionContext, sourceContext) =>
			{
				_ = sourceContext.Match(
					error => AddCompilerError(productionContext, error),
					featureStateClassInfo => Void.Value);
				Console.Beep(11000, 150);
			});

		context.RegisterSourceOutput(
			reducerMethodInfos,
			static (productionContext, sourceContext) =>
			{
				_ = sourceContext.Match(
					error => AddCompilerError(productionContext, error),
					reducerMethodInfo => Void.Value);
				Console.Beep(7000, 150);
			});

		context.RegisterSourceOutput(
			effectMethodInfos,
			static (productionContext, sourceContext) =>
			{
				_ = sourceContext.Match(
					error => AddCompilerError(productionContext, error),
					effectMethodInfo => Void.Value);
				Console.Beep(5000, 150);
			});
	}

	private static Void AddCompilerError(SourceProductionContext productionContext, CompilerError error)
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
	}
}

