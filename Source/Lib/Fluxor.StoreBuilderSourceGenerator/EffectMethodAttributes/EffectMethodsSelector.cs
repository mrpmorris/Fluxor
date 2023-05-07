﻿using Fluxor.StoreBuilderSourceGenerator.Helpers;
using Microsoft.CodeAnalysis;
using System.Threading;

namespace Fluxor.StoreBuilderSourceGenerator.EffectMethodAttributes;

internal static class EffectMethodsSelector
{
	public static IncrementalValuesProvider<Either<CompilerError, EffectMethodInfo>> Select(IncrementalGeneratorInitializationContext context) =>
		context.SyntaxProvider.ForAttributeWithMetadataName(
			fullyQualifiedMetadataName: "Fluxor.EffectMethodAttribute",
			predicate: (SyntaxNode node, CancellationToken cancellationToken) => true,
			transform: (source, cancellationToken) => CreateEffectMethodInfos(source));

	private static Either<CompilerError, EffectMethodInfo> CreateEffectMethodInfos(GeneratorAttributeSyntaxContext context)
	{
		var methodSymbol = context.TargetSymbol as IMethodSymbol;

		string returnTypeClassName = methodSymbol.ReturnType.ToDisplayString();
		if (returnTypeClassName != "System.Threading.Tasks.Task")
			return CompilerError.EffectMethodMustReturnTask with { Location = methodSymbol.Locations[0] };

		string classNamespace = context.TargetSymbol.ContainingNamespace?.ToDisplayString() ?? "";
		string className = methodSymbol.ContainingType.Name;

		string explicitlyDefinedClassFullName = null;

		var attribute = context.Attributes[0];
		if (attribute.ConstructorArguments.Length > 0)
			explicitlyDefinedClassFullName = attribute.ConstructorArguments[0].Value.ToString().Unquote();

		bool requiresActionParameter = explicitlyDefinedClassFullName is null;

		if (requiresActionParameter && methodSymbol.Parameters.Length != 2)
			return CompilerError.EffectMethodMustHaveActionAndIDispatcherParameters with { Location = methodSymbol.Locations[0] };

		if (!requiresActionParameter && methodSymbol.Parameters.Length != 1)
			return CompilerError.EffectMethodWithExplicitlyDefinedActionTypeMustHaveASingleIDispatcherParameter with {  Location = methodSymbol.Locations[0] };

		string actionClassName = explicitlyDefinedClassFullName ?? methodSymbol.Parameters[0].Type.ToDisplayString();
		int dispatcherParameterIndex = requiresActionParameter ? 1 : 0;
		string dispatcherParameterClassName = methodSymbol.Parameters[dispatcherParameterIndex].Type.ToDisplayString();

		if (dispatcherParameterClassName != "Fluxor.IDispatcher")
			return CompilerError.EffectMethodMustHaveAnIDispatcherParameter with {  Location = methodSymbol.Parameters[0].Locations[0] };

		return new EffectMethodInfo(
			ClassNamespace: classNamespace,
			ClassName: className,
			MethodName: context.TargetSymbol.Name,
			ExplicitlyDeclaredActionClassFullName: explicitlyDefinedClassFullName,
			ActionClassFullName: actionClassName,
			IsStatic: methodSymbol.IsStatic);
	}
}

