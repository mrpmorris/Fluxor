using Fluxor.StoreBuilderSourceGenerator.EffectMethodAttributes;
using Fluxor.StoreBuilderSourceGenerator.FeatureStateAttributes;
using Fluxor.StoreBuilderSourceGenerator.DiscoveredMiddlewareClasses;
using Fluxor.StoreBuilderSourceGenerator.ReducerMethodAttributes;
using Microsoft.CodeAnalysis;
using System;
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

		IncrementalValuesProvider<DiscoveredMiddlewareClassInfo> middlewareClassInfos =
			DiscoveredMiddlewareClassesSelector.Select(context);

		context.RegisterSourceOutput(
			middlewareClassInfos,
			static (productionContext, middleware) =>
			{
				Console.WriteLine(middleware.ClassNamespace);
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

		context.RegisterSourceOutput(
			middlewareClassInfos.Collect(),
			static (productionContext, errorOrMiddlewareClasses) =>
			{
				var classes = ImmutableArray.CreateBuilder<DiscoveredMiddlewareClassInfo>();
				for (int i = 0; i < errorOrMiddlewareClasses.Length; i++)
				{
					Either<CompilerError, DiscoveredMiddlewareClassInfo> errorOrMiddlewareClass = errorOrMiddlewareClasses[i];
					_ = errorOrMiddlewareClass.Match
					(
						error => AddCompilerError(productionContext, error),
						middlewareClassInfo =>
						{
							classes.Add(middlewareClassInfo);
							return Void.Value;
						}
					);
				}

				DiscoveredMiddlewareClassGenerator.Generate(productionContext, classes.ToImmutableArray());
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

