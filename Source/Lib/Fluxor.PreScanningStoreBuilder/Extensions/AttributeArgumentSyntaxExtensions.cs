using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Fluxor.PreScanningStoreBuilder.Extensions
{
	internal static class AttributeArgumentSyntaxExtensions
	{
		public static string GetArgumentName(this AttributeArgumentSyntax argument) =>
			argument.NameEquals is not null
			? argument.NameEquals.Name.Identifier.ValueText
			: argument.NameColon is not null
			? argument.NameColon.Name.Identifier.ValueText
			: throw new InvalidOperationException("Cannot find argument value");
	}
}
