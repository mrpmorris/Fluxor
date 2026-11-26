using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace Fluxor.SourceGenerator;

/// <summary>
/// The classes Razor generates from <c>.razor</c> / <c>.cshtml</c> files are added by the Razor source generator,
/// which runs on the same input compilation as this generator, so they are invisible here. Such a class could carry
/// members the reflection scan registers: through <c>@inherits</c> (in the file or an <c>_Imports.razor</c>),
/// <c>@implements</c>, or types and attributed methods in <c>@code</c>. This guard never models Razor output; it
/// reports a problem (the assembly keeps the reflection scan) when a Razor file mentions the name of any type that
/// could hand such members to a class:
/// <list type="bullet">
///   <item>Fluxor's registration attributes and any attribute derived from them;</item>
///   <item>IEffect, IMiddleware, IReducer&lt;T&gt;, IFeature&lt;T&gt; and every interface deriving from them;</item>
///   <item>every non-sealed class implementing one of those interfaces, or with a public
///   <c>[ReducerMethod]</c> / <c>[EffectMethod]</c> method of its own or inherited.</item>
/// </list>
/// Types are looked up in this compilation, Fluxor and every referenced assembly that references Fluxor directly
/// or through other assemblies (no other assembly can declare them). A generic type matches only when its name is
/// followed by <c>&lt;</c>.
/// False positives only cost the speed-up. Code added by other source generators is just as invisible but is not
/// guarded; Docs/generated-registration.md states it as a known limitation.
/// </summary>
internal static class RazorFileGuard
{
	private const string ImportsFileName = "_Imports";

	public static IEnumerable<RegistrationProblem> GetProblems(
		Compilation compilation,
		FluxorSymbols symbols,
		ImmutableArray<AdditionalText> razorFiles,
		CancellationToken cancellationToken)
	{
		if (razorFiles.IsEmpty)
			yield break;

		var nonGenericNames = new HashSet<string>(StringComparer.Ordinal);
		var genericNames = new HashSet<string>(StringComparer.Ordinal);
		foreach (INamedTypeSymbol type in GetTypesToLookAt(compilation, symbols))
		{
			cancellationToken.ThrowIfCancellationRequested();
			foreach (string name in GetNamesIfRelevant(type, symbols))
				(type.Arity > 0 ? genericNames : nonGenericNames).Add(name);
		}

		var flaggedFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		bool isImportsFileFlagged = false;
		foreach (AdditionalText file in razorFiles)
		{
			string? text = file.GetText(cancellationToken)?.ToString();
			string? mentionedName = text is null ? null : FindMentionedName(text, nonGenericNames, genericNames);
			if (mentionedName is null)
				continue;

			string fileName = GetFileNameWithoutExtension(file.Path);
			flaggedFileNames.Add(fileName);
			isImportsFileFlagged |= string.Equals(fileName, ImportsFileName, StringComparison.OrdinalIgnoreCase);
			yield return new RegistrationProblem(
				file.Path + " (a Razor file that mentions " + mentionedName
				+ "; the classes Razor generates are not visible to this generator)",
				Location.None);
		}

		if (flaggedFileNames.Count == 0)
			yield break;

		// A code-behind partial class is only half visible here: the Razor half can give it a base class and interfaces
		foreach (INamedTypeSymbol type in RegistrationModelBuilder.GetDeclaredTypes(compilation.Assembly.GlobalNamespace))
		{
			if (!IsPartialClass(type, cancellationToken))
				continue;
			if (!isImportsFileFlagged && !flaggedFileNames.Contains(type.Name))
				continue;

			yield return new RegistrationProblem(
				type.ToDisplayString() + " (a partial class that may be completed by a Razor file that mentions Fluxor types)",
				type.Locations.FirstOrDefault(x => x.IsInSource) ?? Location.None);
		}
	}

	private static IEnumerable<INamedTypeSymbol> GetTypesToLookAt(Compilation compilation, FluxorSymbols symbols)
	{
		IEnumerable<IAssemblySymbol> assemblies = new[] { compilation.Assembly }
			.Concat(GetAssembliesReachingFluxor(compilation, symbols.FeatureInterface.ContainingAssembly));

		foreach (IAssemblySymbol assembly in assemblies)
		{
			bool isOwnAssembly = SymbolEqualityComparer.Default.Equals(assembly, compilation.Assembly);
			foreach (INamedTypeSymbol type in RegistrationModelBuilder.GetDeclaredTypes(assembly.GlobalNamespace))
			{
				if (isOwnAssembly || compilation.IsSymbolAccessibleWithin(type, compilation.Assembly))
					yield return type;
			}
		}
	}

	/// <summary>
	/// Every referenced assembly that is Fluxor or references it, directly or through other
	/// assemblies: only those can declare a type that gets Fluxor members from a base class, an interface or an
	/// attribute. An assembly's metadata only references the assemblies of types it names itself, so a class
	/// deriving from a Fluxor-related class of another assembly has no reference to Fluxor of its own; the
	/// references are therefore followed transitively.
	/// </summary>
	private static IEnumerable<IAssemblySymbol> GetAssembliesReachingFluxor(Compilation compilation, IAssemblySymbol fluxor)
	{
		// Every assembly the compiler resolved, the ones only referenced by other references included
		ImmutableArray<IAssemblySymbol> all = compilation.SourceModule.ReferencedAssemblySymbols;
		var reaching = new HashSet<IAssemblySymbol>(
			all.Where(x => x.Identity.Name == fluxor.Identity.Name),
			SymbolEqualityComparer.Default);
		bool added;
		do
		{
			added = false;
			foreach (IAssemblySymbol assembly in all)
			{
				if (!reaching.Contains(assembly)
					&& assembly.Modules.Any(m => m.ReferencedAssemblySymbols.Any(reaching.Contains)))
				{
					reaching.Add(assembly);
					added = true;
				}
			}
		}
		while (added);

		return all.Where(reaching.Contains);
	}

	private static IEnumerable<string> GetNamesIfRelevant(INamedTypeSymbol type, FluxorSymbols symbols)
	{
		if (type.TypeKind == TypeKind.Class
			&& (FluxorSymbols.IsOrDerivesFrom(type, symbols.FeatureStateAttribute)
				|| FluxorSymbols.IsOrDerivesFrom(type, symbols.ReducerMethodAttribute)
				|| FluxorSymbols.IsOrDerivesFrom(type, symbols.EffectMethodAttribute)))
		{
			yield return type.Name;
			if (type.Name.EndsWith("Attribute", StringComparison.Ordinal) && type.Name.Length > "Attribute".Length)
				yield return type.Name.Substring(0, type.Name.Length - "Attribute".Length);
			yield break;
		}

		if (type.TypeKind == TypeKind.Interface)
		{
			if (IsRegistrationInterface(type, symbols) || type.AllInterfaces.Any(x => IsRegistrationInterface(x, symbols)))
				yield return type.Name;
			yield break;
		}

		if (type.TypeKind != TypeKind.Class || type.IsSealed || type.IsStatic)
			yield break;

		if (type.AllInterfaces.Any(x => IsRegistrationInterface(x, symbols))
			|| RegistrationModelBuilder.GetScannedMethods(type).Any(x => HasRegistrationAttribute(x, symbols)))
			yield return type.Name;
	}

	private static bool IsRegistrationInterface(INamedTypeSymbol type, FluxorSymbols symbols) =>
		SymbolEqualityComparer.Default.Equals(type, symbols.EffectInterface)
		|| SymbolEqualityComparer.Default.Equals(type, symbols.MiddlewareInterface)
		|| SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, symbols.ReducerInterface)
		|| SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, symbols.FeatureInterface);

	private static bool HasRegistrationAttribute(IMethodSymbol method, FluxorSymbols symbols) =>
		method.GetAttributes().Any(x =>
			FluxorSymbols.IsOrDerivesFrom(x.AttributeClass, symbols.ReducerMethodAttribute)
			|| FluxorSymbols.IsOrDerivesFrom(x.AttributeClass, symbols.EffectMethodAttribute));

	/// <summary>
	/// The first identifier in the text that names a relevant type; a generic name must be followed by
	/// <c>&lt;</c> (whitespace allowed in between).
	/// </summary>
	private static string? FindMentionedName(string text, HashSet<string> nonGenericNames, HashSet<string> genericNames)
	{
		int index = 0;
		while (index < text.Length)
		{
			if (!SyntaxFacts.IsIdentifierStartCharacter(text[index]))
			{
				index++;
				continue;
			}

			int start = index;
			while (index < text.Length && SyntaxFacts.IsIdentifierPartCharacter(text[index]))
				index++;
			string identifier = text.Substring(start, index - start);

			if (nonGenericNames.Contains(identifier))
				return identifier;

			if (genericNames.Contains(identifier))
			{
				int next = index;
				while (next < text.Length && char.IsWhiteSpace(text[next]))
					next++;
				if (next < text.Length && text[next] == '<')
					return identifier;
			}
		}
		return null;
	}

	private static bool IsPartialClass(INamedTypeSymbol type, CancellationToken cancellationToken) =>
		type.TypeKind == TypeKind.Class
		&& type.DeclaringSyntaxReferences.Any(x =>
			x.GetSyntax(cancellationToken) is TypeDeclarationSyntax declaration
			&& declaration.Modifiers.Any(SyntaxKind.PartialKeyword));

	private static string GetFileNameWithoutExtension(string path)
	{
		int start = Math.Max(path.LastIndexOf('/'), path.LastIndexOf('\\')) + 1;
		int end = path.LastIndexOf('.');
		return end > start ? path.Substring(start, end - start) : path.Substring(start);
	}
}
