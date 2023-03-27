using Fluxor.PreScanningStoreBuilder.Extensions;
using Fluxor.PreScanningStoreBuilder.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Fluxor.PreScanningStoreBuilder.CodeModels
{
	internal readonly struct ClassInfoWithAttributeInfo: IEquatable<ClassInfoWithAttributeInfo>
	{
		public readonly bool IsEmpty;
		public readonly ClassInfo ClassInfo;
		public readonly ImmutableArray<KeyValuePair<string, string>> Attributes;

		public ClassInfoWithAttributeInfo(GeneratorSyntaxContext syntaxContext, string attributeFullName)
		{
			var classDeclarationSyntax = (ClassDeclarationSyntax)syntaxContext.Node;
			AttributeSyntax matchingAttribute = classDeclarationSyntax
				.AttributeLists
				.SelectMany(list => list.Attributes)
				.Where(x => syntaxContext.SemanticModel.GetTypeInfo(x).ConvertedType.ToDisplayString() == attributeFullName)
				.FirstOrDefault();

			IsEmpty = matchingAttribute is null;
			if (!IsEmpty)
			{
				ClassInfo = new ClassInfo(syntaxContext);
				Attributes = 
					matchingAttribute.ArgumentList is null
					? ImmutableArray<KeyValuePair<string, string>>.Empty
					: matchingAttribute
							.ArgumentList
							.Arguments
							.Select(x => 
								new KeyValuePair<string, string>(
									x.GetArgumentName(),
									x.Expression.ToFullString())
							).ToImmutableArray();
			}
		}

		public override bool Equals(object obj) =>
			obj switch {
				ClassInfoWithAttributeInfo other => Equals(other),
				_ => false
			};

		public bool Equals(ClassInfoWithAttributeInfo other) =>
			other.ClassInfo == ClassInfo
			&&
			(
				other.IsEmpty == IsEmpty
				|| other.Attributes.SequenceEqual(Attributes)
			);

		public override int GetHashCode() => HashCode.Combine(ClassInfo);
		public static bool operator ==(ClassInfoWithAttributeInfo left, ClassInfoWithAttributeInfo right) => left.Equals(right);
		public static bool operator !=(ClassInfoWithAttributeInfo left, ClassInfoWithAttributeInfo right) => !left.Equals(right);
	}
}
