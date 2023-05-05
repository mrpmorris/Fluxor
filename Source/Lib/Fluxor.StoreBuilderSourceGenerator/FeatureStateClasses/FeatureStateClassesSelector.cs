﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Fluxor.StoreBuilderSourceGenerator.FeatureStateClasses;

internal static class FeatureStateClassesSelector
{
	public static IncrementalValuesProvider<FeatureStateClassInfo> Select(IncrementalGeneratorInitializationContext context) =>
		context.SyntaxProvider.ForAttributeWithMetadataName(
			fullyQualifiedMetadataName: "Fluxor.FeatureStateAttribute",
			predicate: (SyntaxNode node, CancellationToken cancellationToken) => true,
			transform: (source, cancellationToken) => CreateFeatureStateClassInfos(source));

	private static FeatureStateClassInfo CreateFeatureStateClassInfos(GeneratorAttributeSyntaxContext context)
	{
		string classNamespace = context.TargetSymbol.ContainingNamespace?.ToDisplayString() ?? "";
		string className = context.TargetSymbol.Name;
		string stateName = className;
		string createInitialStateMethodName = null;
		string maximumStateChangedNotificationsPerSecond = "0";

		var attribute = context.Attributes[0];
		for (int argIndex = 0; argIndex < attribute.NamedArguments.Length; argIndex++)
		{
			KeyValuePair<string, TypedConstant> argument = attribute.NamedArguments[argIndex];
			switch (argument.Key)
			{
				case "CreateInitialStateMethodName":
					createInitialStateMethodName = argument.Value.ToCSharpString().Unquote();
					break;

				case "MaximumStateChangedNotificationsPerSecond":
					maximumStateChangedNotificationsPerSecond = argument.Value.ToCSharpString();
					break;

				case "Name":
					stateName = argument.Value.ToCSharpString().Unquote();
					break;

				default: throw new NotImplementedException(argument.Key);
			}
		}
		return new FeatureStateClassInfo(
			ClassNamespace: classNamespace,
			ClassName: className,
			StateName: stateName,
			CreateInitialStateMethodName: createInitialStateMethodName,
			MaximumStateChangedNotificationsPerSecond: byte.Parse(maximumStateChangedNotificationsPerSecond));
	}
}

