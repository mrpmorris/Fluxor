using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Fluxor.Reactor.Maui.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CallBaseOnMounted : DiagnosticAnalyzer
{
	private static readonly DiagnosticDescriptor Rule = new(
		id: "FLXM01",
		title: "Base method not called",
		messageFormat: "Overriding OnMounted without calling base is not allowed",
		category: "Usage",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
		context.EnableConcurrentExecution();
		context.RegisterCompilationStartAction(context =>
		{
			var fluxorComponent = context.Compilation.GetTypeByMetadataName("Fluxor.Reactor.Maui.Components.FluxorComponent");
			context.RegisterOperationBlockAction(context => AnalyzeOperationBlock(context, fluxorComponent));
		});
	}

	private void AnalyzeOperationBlock(OperationBlockAnalysisContext context, INamedTypeSymbol? fluxorComponent)
	{
		if (context.OwningSymbol is not IMethodSymbol { IsOverride: true, Name: "OnMounted" } method ||
			!IsFluxorComponentBase(method.ContainingType, fluxorComponent))
		{
			return;
		}

		if (!CallsBase(context.OperationBlocks, method.OverriddenMethod))
			context.ReportDiagnostic(Diagnostic.Create(Rule, method.Locations[0]));
	}

	private bool CallsBase(ImmutableArray<IOperation> operations, IMethodSymbol? overriddenMethod)
	{
		foreach (var operation in operations)
		{
			if (CallsBase(operation, overriddenMethod))
				return true;
		}

		return false;
	}

	private bool CallsBase(IOperation? operation, IMethodSymbol? overriddenMethod)
	{
		if (operation is null)
			return false;

		if (operation is IBlockOperation blockOperation)
			return CallsBase(blockOperation.Operations, overriddenMethod);

		if (operation is IReturnOperation returnOperation)
			return CallsBase(returnOperation.ReturnedValue, overriddenMethod);

		if (operation is IExpressionStatementOperation expressionStatementOperation)
			operation = expressionStatementOperation.Operation;

		if (operation is IAwaitOperation awaitOperation)
			operation = awaitOperation.Operation;

		return operation is IInvocationOperation invocation &&
			invocation.TargetMethod.Equals(overriddenMethod, SymbolEqualityComparer.Default);
	}

	private static bool IsFluxorComponentBase(INamedTypeSymbol symbol, INamedTypeSymbol? fluxorComponent)
		=> DerivesFrom(symbol, fluxorComponent);

	private static bool DerivesFrom(INamedTypeSymbol symbol, INamedTypeSymbol? candidateBaseType)
	{
		var baseType = symbol.BaseType;
		while (baseType is not null)
		{
			if (SymbolEqualityComparer.Default.Equals(baseType, candidateBaseType))
				return true;

			baseType = baseType.BaseType;
		}

		return false;
	}
}
