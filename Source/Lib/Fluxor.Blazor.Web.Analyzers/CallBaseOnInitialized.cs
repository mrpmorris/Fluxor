using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Fluxor.Blazor.Web.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CallBaseOnInitialized : DiagnosticAnalyzer
{
	private static readonly DiagnosticDescriptor Rule = new(
		id: "FLXW01",
		title: "Base method not called",
		messageFormat: "Overriding OnInitialized or OnInitializedAsync without calling base is not allowed",
		category: "Usage",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		context.RegisterSyntaxNodeAction(
			action: AnalyzeMethod,
			syntaxKinds: SyntaxKind.MethodDeclaration);
	}

	private static bool IsFluxorComponentBase(INamedTypeSymbol? symbol) =>
		symbol is not null
		&& (
			IsFluxorType(symbol, "Fluxor.Blazor.Web.Components.FluxorComponent")
			|| IsFluxorType(symbol, "Fluxor.Blazor.Web.Components.FluxorLayout")
		);

	private static bool IsFluxorType(INamedTypeSymbol symbol, string fullyQualifiedName) =>
		symbol.ToString() == fullyQualifiedName
		|| (
			symbol.BaseType is not null
			&& IsFluxorType(symbol.BaseType, fullyQualifiedName)
		);

	private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
	{
		if (context.Node is not MethodDeclarationSyntax methodDecl)
			return;

		if (methodDecl.Modifiers.All(m => m.Text != "override"))
			return;

		string methodName = methodDecl.Identifier.Text;
		if (methodName != "OnInitialized" && methodName != "OnInitializedAsync")
			return;

		SemanticModel semanticModel = context.SemanticModel;
		INamedTypeSymbol? containingType = semanticModel.GetDeclaredSymbol(methodDecl)?.ContainingType;
		if (containingType is null)
			return;

		if (!IsFluxorComponentBase(containingType))
			return;

		bool callsBase =
			methodDecl.Body is not null
			&& methodDecl.Body.Statements
				.OfType<ExpressionStatementSyntax>()
				.Select(s => s.Expression).OfType<InvocationExpressionSyntax>()
				.Any(inv => IsBaseCall(inv, methodName, semanticModel));

		if (!callsBase && methodDecl.ExpressionBody is null)
		{
			context.ReportDiagnostic(Diagnostic.Create(Rule, methodDecl.Identifier.GetLocation()));
			return;
		}

		if (methodDecl.ExpressionBody is not null)
		{
			ExpressionSyntax expr = methodDecl.ExpressionBody.Expression;
			if (
				expr is InvocationExpressionSyntax exprInv
				&& IsBaseCall(exprInv, methodName, semanticModel)
			)
			{
				return;
			}
			context.ReportDiagnostic(Diagnostic.Create(Rule, methodDecl.Identifier.GetLocation()));
		}
	}

	private static bool IsBaseCall(
		InvocationExpressionSyntax invocation,
		string methodName,
		SemanticModel semanticModel)
	{
		if (
			invocation.Expression is MemberAccessExpressionSyntax memberAccess
			&& memberAccess.Expression is BaseExpressionSyntax)
		{
			var symbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
			if (symbol is not null && symbol.Name == methodName)
				return true;
		}
		return false;
	}
}
