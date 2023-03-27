using Fluxor.PreScanningStoreBuilder.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Fluxor.PreScanningStoreBuilder.CodeModels
{
	internal readonly struct ClassInfo : IEquatable<ClassInfo>
	{
		public readonly string Namespace;
		public readonly string Name;
		public readonly string FullName;
		public readonly ImmutableArray<string> GenericParameterNames;
		public readonly INamedTypeSymbol NamedTypeSymbol;

		public ClassInfo(GeneratorSyntaxContext syntaxContext)
		{
			var classDeclarationSyntax = (ClassDeclarationSyntax)syntaxContext.Node;

			NamedTypeSymbol = (INamedTypeSymbol)syntaxContext.SemanticModel.GetDeclaredSymbol(syntaxContext.Node);
			Name = classDeclarationSyntax.Identifier.Text;

			Namespace = NamedTypeSymbol.ContainingNamespace.IsGlobalNamespace
				? string.Empty
				: NamedTypeSymbol.ContainingNamespace.ToString();

			FullName =
				Namespace == string.Empty
				? Name
				: $"{Namespace}.{Name}";

			GenericParameterNames =
				classDeclarationSyntax.TypeParameterList?.Parameters.Count > 0
				? classDeclarationSyntax.TypeParameterList.Parameters.Select(x => x.Identifier.Text).ToImmutableArray()
				: ImmutableArray<string>.Empty;
		}

		public override bool Equals(object obj) =>
			obj switch {
				ClassInfo other => Equals(other),
				_ => false
			};

		public bool Equals(ClassInfo other) =>
			other.FullName == FullName
			&& other.GenericParameterNames.SequenceEqual(GenericParameterNames)
			&& other.NamedTypeSymbol.Equals(NamedTypeSymbol, SymbolEqualityComparer.Default);

		public override int GetHashCode() =>
			HashCode.Combine(FullName, GenericParameterNames, NamedTypeSymbol);

		public static bool operator ==(ClassInfo left, ClassInfo right) => left.Equals(right);
		public static bool operator !=(ClassInfo left, ClassInfo right) => !left.Equals(right);
	}
}
