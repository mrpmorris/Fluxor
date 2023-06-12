using Fluxor.StoreBuilderSourceGenerator.Helpers;
using Microsoft.CodeAnalysis;
using System.Threading;

namespace Fluxor.StoreBuilderSourceGenerator.ReducerMethodAttributes;

internal static class ReducerMethodsSelector
{
	public static IncrementalValuesProvider<Either<CompilerError, ReducerMethodInfo>> Select(IncrementalGeneratorInitializationContext context) =>
		context.SyntaxProvider.ForAttributeWithMetadataName(
			fullyQualifiedMetadataName: "Fluxor.ReducerMethodAttribute",
			predicate: (SyntaxNode node, CancellationToken cancellationToken) => true,
			transform: (source, cancellationToken) => CreateReducerMethodInfos(source));

	private static Either<CompilerError, ReducerMethodInfo> CreateReducerMethodInfos(GeneratorAttributeSyntaxContext context)
	{
		var methodSymbol = context.TargetSymbol as IMethodSymbol;
		if (!methodSymbol.IsStatic)
			return CompilerError.ReducerMethodMustBeStatic with { Location = methodSymbol.Locations[0] };

		string returnTypeClassFullName = methodSymbol.ReturnType.ToDisplayString();
		if (returnTypeClassFullName == "void")
			return CompilerError.ReducerMethodMustReturnState with { Location = methodSymbol.Locations[0] };

		string classNamespace = context.TargetSymbol.ContainingNamespace?.ToDisplayString() ?? "";
		string className = methodSymbol.ContainingType.Name;

		string explicitlyDefinedActionClassFullName = null;

		var attribute = context.Attributes[0];
		if (attribute.ConstructorArguments.Length > 0)
			explicitlyDefinedActionClassFullName = attribute.ConstructorArguments[0].Value.ToString().Unquote();

		bool requiresActionParameter = explicitlyDefinedActionClassFullName is null;

		if (requiresActionParameter && methodSymbol.Parameters.Length != 2)
			return CompilerError.ReducerMethodMustHaveStateAndActionParameters with { Location = methodSymbol.Locations[0] };

		if (!requiresActionParameter && methodSymbol.Parameters.Length != 1)
			return CompilerError.ReducerMethodWithExplicitlyDefinedActionTypeMustHaveASingleStateParameter with {  Location = methodSymbol.Locations[0] };

		string stateClassFullName = methodSymbol.Parameters[0].Type.ToDisplayString();
		string actionClassFullName = explicitlyDefinedActionClassFullName ?? methodSymbol.Parameters[1].Type.ToDisplayString();

		if (returnTypeClassFullName != stateClassFullName)
			return CompilerError.ReducerMethodsReceivedStateTypeMustBeTheSameAsTheMethodsReturnType with {  Location = methodSymbol.Parameters[0].Locations[0] };

		return new ReducerMethodInfo(
			ClassName: className,
			ClassNamespace: classNamespace,
			MethodName: context.TargetSymbol.Name,
			StateClassFullName: stateClassFullName,
			ExplicitlyDeclaredActionClassFullName: explicitlyDefinedActionClassFullName,
			ActionClassFullName: actionClassFullName);
	}
}

