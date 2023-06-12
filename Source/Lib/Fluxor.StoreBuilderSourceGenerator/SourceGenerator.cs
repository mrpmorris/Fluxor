using Fluxor.StoreBuilderSourceGenerator.EffectMethodAttributes;
using Fluxor.StoreBuilderSourceGenerator.FeatureStateAttributes;
using Fluxor.StoreBuilderSourceGenerator.DiscoveredMiddlewareClasses;
using Fluxor.StoreBuilderSourceGenerator.ReducerMethodAttributes;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Linq;

namespace Fluxor.StoreBuilderSourceGenerator;

[Generator]
public partial class SourceGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		IncrementalValueProvider<string> rootNamespace = GetRootNameSpace(context);
		IncrementalValuesProvider<Either<CompilerError, EffectMethodInfo>> effectMethodInfos = GenerateEffectClassesForEffectMethods(context);
		IncrementalValuesProvider<Either<CompilerError, FeatureStateClassInfo>> featureStateClassInfos = GenerateFeatureClassesForFeatureStateClasses(context);
		IncrementalValuesProvider<Either<CompilerError, ReducerMethodInfo>> reducerMethodInfos = GenerateReducerClassesForReducerMethods(context);

		IncrementalValueProvider<ImmutableArray<string>> discoveredEffectClassNames = EffectClassesSelector
			.Select(context)
			.Collect();

		IncrementalValueProvider<ImmutableArray<string>> discoveredFeatureClassNames = FeatureClassesSelector
			.Select(context)
			.Collect();

		IncrementalValueProvider<ImmutableArray<string>> discoveredMiddlewareClassNames = MiddlewareClassesSelector
			.Select(context)
			.Collect();

		IncrementalValueProvider<ImmutableArray<string>> discoveredReducerClassNames = ReducerClassesSelector
			.Select(context)
			.Collect();

		var discoveredClasses =
			discoveredEffectClassNames
			.Combine(discoveredFeatureClassNames)
			.Select((x, _) =>
				new DiscoveredClasses {
					DiscoveredEffectClassNames = x.Left,
					DiscoveredFeatureClassNames = x.Right
				})
			.Combine(discoveredMiddlewareClassNames)
			.Select((x, _) =>
				x.Left with {
					DiscoveredMiddlewareClassNames = x.Right
				})
			.Combine(discoveredReducerClassNames)
			.Select((x, _) =>
				x.Left with {
					DiscoveredReducerClassNames = x.Right
				})
			.Combine(effectMethodInfos.Collect())
			.Select((x, _) =>
				x.Left with {
					EffectMethodInfos = x.Right
						.Where(x => x.IsRight)
						.Select(x => x.Right)
						.ToImmutableArray()
				})
			.Combine(featureStateClassInfos.Collect())
			.Select((x, _) =>
				x.Left with {
					FeatureStateClassInfos = x.Right
						.Where(x => x.IsRight)
						.Select(x => x.Right)
						.ToImmutableArray()
				})
			.Combine(reducerMethodInfos.Collect())
			.Select((x, _) =>
				x.Left with {
					ReducerMethodInfos = x.Right
						.Where(x => x.IsRight)
						.Select(x => x.Right)
						.ToImmutableArray()
				})
			.Combine(rootNamespace);

		context.RegisterSourceOutput(
			discoveredClasses,
			static (productionContext, source) =>
			{
				var discoveredClasses = source.Left;
				string rootNamespace = source.Right;
				FluxorModuleGenerator.Generate(productionContext, rootNamespace, discoveredClasses);
			});
	}

	private static IncrementalValueProvider<string> GetRootNameSpace(IncrementalGeneratorInitializationContext context) =>
		context.AnalyzerConfigOptionsProvider.Select((x, _) =>
			x.GlobalOptions.TryGetValue("build_property.RootNamespace", out string value) ? value : "");

	private static IncrementalValuesProvider<Either<CompilerError, EffectMethodInfo>> GenerateEffectClassesForEffectMethods(IncrementalGeneratorInitializationContext context)
	{
		IncrementalValuesProvider<Either<CompilerError, EffectMethodInfo>> effectMethodInfos =
			EffectMethodsSelector.Select(context);

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
		return effectMethodInfos;
	}

	private static IncrementalValuesProvider<Either<CompilerError, FeatureStateClassInfo>> GenerateFeatureClassesForFeatureStateClasses(IncrementalGeneratorInitializationContext context)
	{
		IncrementalValuesProvider<Either<CompilerError, FeatureStateClassInfo>> featureStateClassInfos =
			FeatureStateClassesSelector.Select(context);

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
		return featureStateClassInfos;
	}

	private static IncrementalValuesProvider<Either<CompilerError, ReducerMethodInfo>> GenerateReducerClassesForReducerMethods(IncrementalGeneratorInitializationContext context)
	{
		IncrementalValuesProvider<Either<CompilerError, ReducerMethodInfo>> reducerMethodInfos =
			ReducerMethodsSelector.Select(context);

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
		return reducerMethodInfos;
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

