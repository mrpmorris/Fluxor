using Fluxor.StoreBuilderSourceGenerator.EffectMethodAttributes;
using Fluxor.StoreBuilderSourceGenerator.FeatureStateAttributes;
using Fluxor.StoreBuilderSourceGenerator.DiscoveredMiddlewareClasses;
using Fluxor.StoreBuilderSourceGenerator.ReducerMethodAttributes;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

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

		IncrementalValueProvider<ImmutableArray<string>> effectClassNames = EffectClassesSelector
			.Select(context)
			.Collect();

		IncrementalValueProvider<ImmutableArray<string>> featureClassNames = FeatureClassesSelector
			.Select(context)
			.Collect();

		IncrementalValueProvider<ImmutableArray<string>> middlewareClassNames = MiddlewareClassesSelector
			.Select(context)
			.Collect();

		IncrementalValueProvider<ImmutableArray<string>> reducerClassNames = ReducerClassesSelector
			.Select(context)
			.Collect();

		var discoveredClasses =
			effectClassNames
			.Combine(featureClassNames)
			.Select((x, _) =>
				new
				{
					EffectClassNames = x.Left,
					FeatureClassNames = x.Right
				})
			.Combine(middlewareClassNames)
			.Select((x, _) =>
				new
				{
					x.Left.EffectClassNames,
					x.Left.FeatureClassNames,
					MiddlewareClassNames = x.Right
				})
			.Combine(reducerClassNames)
			.Select((x, _) =>
				new
				{
					x.Left.EffectClassNames,
					x.Left.FeatureClassNames,
					x.Left.MiddlewareClassNames,
					ReducerClassNames = x.Right
				});

		//TODO: PeteM Add in generated names

		context.RegisterSourceOutput(
			discoveredClasses,
			static (productionContext, discoveredClasses) =>
			{
			});

		context.RegisterSourceOutput(
			featureStateClassInfos,
			static (productionContext, errorOrFeature) =>
			{
				_ = errorOrFeature.Match
				(
					error => AddCompilerError(productionContext, error),
					featureStateClassInfo => FeatureGenerator.Generate(productionContext, featureStateClassInfo)
				);
			});

		context.RegisterSourceOutput(
			reducerMethodInfos,
			static (productionContext, errorOrReducerMethod) =>
			{
				_ = errorOrReducerMethod.Match
				(
					error => AddCompilerError(productionContext, error),
					reducerMethodInfo => ReducerGenerator.Generate(productionContext, reducerMethodInfo)
				);
			});

		context.RegisterSourceOutput(
			effectMethodInfos,
			static (productionContext, errorOrEffectMethod) =>
			{
				_ = errorOrEffectMethod.Match
				(
					error => AddCompilerError(productionContext, error),
					effectMethodInfo => EffectGenerator.Generate(productionContext, effectMethodInfo)
				);
			});



	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
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

