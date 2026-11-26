using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using Xunit;

namespace Fluxor.SourceGenerator.UnitTests;

/// <summary>
/// Which Razor files and code-behind partial classes make the generator fall back to the reflection scan.
/// </summary>
public class RazorFileGuardTests
{
	private const string GeneratedFileName = "FluxorGeneratedRegistration.g.cs";

	private const string Source = """
		using Fluxor;
		using System.Threading.Tasks;
		namespace Ns;
		public record S;
		public record A;
		public class ComponentWithReducers
		{
			[ReducerMethod]
			public static S Reduce(S state, A action) => state;
		}
		public sealed class SealedReducers
		{
			[ReducerMethod]
			public static S Reduce(S state, A action) => state;
		}
		public static class StaticReducers
		{
			[ReducerMethod]
			public static S Reduce(S state, A action) => state;
		}
		// Derives from ReducerMethodAttribute without the Attribute suffix
		public class ReducerOfMine : ReducerMethodAttribute { }
		public partial class Counter { }
		public partial class Unrelated { }
		""";

	[Theory]
	[InlineData("@code { [EffectMethodAttribute] public static Task Handle(A a, IDispatcher d) => Task.CompletedTask; }", "EffectMethodAttribute")]
	[InlineData("@code { [EffectMethod] public static Task Handle(A a, IDispatcher d) => Task.CompletedTask; }", "EffectMethod")]
	[InlineData("@code { [ReducerOfMine] public static S Reduce(S s, A a) => s; }", "ReducerOfMine")]
	[InlineData("@code { void M() { Reducer<S, A> r = null; } }", "Reducer")]
	[InlineData("@code { void M() { Reducer\t\n <S, A> r = null; } }", "Reducer")]
	public void WhenRazorFileMentionsFluxorRegistrationType_ThenItIsReported(string razorText, string mentionedName)
	{
		ImmutableArray<Diagnostic> diagnostics = Run(Source, ("Pages/Other.razor", razorText));

		Assert.Equal(
			new[] { "Pages/Other.razor (a Razor file that mentions " + mentionedName + "; the classes Razor generates are not visible to this generator)" },
			Members(diagnostics));
	}

	[Theory]
	[InlineData("@inherits SealedReducers")]
	[InlineData("@code { int x = StaticReducers.Count; }")]
	[InlineData("@code { int Redu; int Reducer; }")]
	[InlineData("<p>The end is a generic name without a type argument list: Reducer")]
	[InlineData("<p>Then whitespace: Reducer \n\t")]
	public void WhenRazorFileMentionsNoInheritableRegistrationType_ThenRegistrationIsGenerated(string razorText)
	{
		(Compilation output, ImmutableArray<Diagnostic> diagnostics) = RunWithOutput(Source, ("Pages/Other.razor", razorText));

		Assert.Empty(diagnostics);
		Assert.Contains(output.SyntaxTrees, x => x.FilePath.EndsWith(GeneratedFileName, StringComparison.Ordinal));
	}

	[Fact]
	public void WhenRazorFileIsFlagged_ThenOnlyThePartialClassWithItsNameIsReported()
	{
		const string nonPartialCounter = """
			namespace Other;
			public class Counter { }
			""";

		ImmutableArray<Diagnostic> diagnostics = Run(
			new[] { Source, nonPartialCounter },
			("Pages/Counter.razor", "@inherits ComponentWithReducers"));

		Assert.Equal(
			new[]
			{
				"Pages/Counter.razor (a Razor file that mentions ComponentWithReducers; the classes Razor generates are not visible to this generator)",
				"Ns.Counter (a partial class that may be completed by a Razor file that mentions Fluxor types)",
			},
			Members(diagnostics));
		Diagnostic partialClass = diagnostics[1];
		Assert.True(partialClass.Location.IsInSource);
		Assert.Equal("Counter", partialClass.Location.SourceTree.GetText().ToString(partialClass.Location.SourceSpan));
	}

	[Fact]
	public void WhenEveryFlaggedFileIsAnImportsFile_ThenEveryPartialClassIsReported()
	{
		ImmutableArray<Diagnostic> diagnostics = Run(
			new[] { Source },
			("Pages/_Imports.razor", "@inherits ComponentWithReducers"),
			("Shared/_IMPORTS.razor", "@inherits ComponentWithReducers"));

		string[] members = Members(diagnostics);
		Assert.Contains("Ns.Counter (a partial class that may be completed by a Razor file that mentions Fluxor types)", members);
		Assert.Contains("Ns.Unrelated (a partial class that may be completed by a Razor file that mentions Fluxor types)", members);
	}

	[Fact]
	public void WhenRazorFileCannotBeRead_ThenItIsIgnored()
	{
		var unreadable = new UnreadableText("Pages/Counter.razor");

		(Compilation output, ImmutableArray<Diagnostic> diagnostics) = GeneratorTestHost.Run(
			new[] { Source },
			"true",
			additionalTexts: new AdditionalText[] { unreadable });

		Assert.Empty(diagnostics);
		Assert.Contains(output.SyntaxTrees, x => x.FilePath.EndsWith(GeneratedFileName, StringComparison.Ordinal));
	}

	[Fact]
	public void WhenRazorFileMentionsAClassOfAReferencedAssembly_ThenItIsReported()
	{
		const string referencedSource = """
			using Fluxor;
			namespace Referenced;
			public record RefState;
			public record RefAction;
			public class ReferencedComponentBase
			{
				[ReducerMethod]
				public static RefState Reduce(RefState state, RefAction action) => state;
			}
			""";
		Compilation referenced = GeneratorTestHost.CreateCompilation(new[] { referencedSource }, "Fluxor.SourceGenerator.ReferencedAssembly");

		(_, ImmutableArray<Diagnostic> diagnostics) = GeneratorTestHost.Run(
			new[] { "namespace Ns; public class Plain { }" },
			"true",
			new[] { GeneratorTestHost.CreateReference(referenced) },
			additionalTexts: new[] { new GeneratorTestHost.InMemoryAdditionalText("Pages/Counter.razor", "@inherits Referenced.ReferencedComponentBase") });

		Assert.Equal(
			new[] { "Pages/Counter.razor (a Razor file that mentions ReferencedComponentBase; the classes Razor generates are not visible to this generator)" },
			Members(diagnostics));
	}

	/// <summary>
	/// C derives from a Fluxor-related class of B but names no Fluxor type itself, so C's metadata has no
	/// reference to Fluxor. Its classes can still hand B's registered members to a Razor component.
	/// </summary>
	[Theory]
	[InlineData("public class ReducerComponentBase { [ReducerMethod] public static RefState Reduce(RefState state, RefAction action) => state; }", "ReducerComponentBase", false)]
	[InlineData("public class ReducerComponentBase { [ReducerMethod] public static RefState Reduce(RefState state, RefAction action) => state; }", "ReducerComponentBase", true)]
	[InlineData("public class EffectComponentBase : IEffect { public Task HandleAsync(object a, IDispatcher d) => Task.CompletedTask; public bool ShouldReactToAction(object a) => false; }", "EffectComponentBase", false)]
	public void WhenRazorFileMentionsAClassWhoseAssemblyReachesFluxorOnlyThroughAnotherAssembly_ThenItIsReported(
		string baseClass,
		string baseName,
		bool referenceCBeforeB)
	{
		(Compilation bCompilation, Compilation cCompilation) = CreateReferenceChain(baseClass, baseName);
		MetadataReference b = GeneratorTestHost.CreateReference(bCompilation);
		MetadataReference c = GeneratorTestHost.CreateReference(cCompilation);
		// The premise: C's metadata references B, not Fluxor
		Compilation probe = GeneratorTestHost.CreateCompilation(new[] { "class P { }" }, additionalReferences: new[] { b, c });
		var cAssembly = (IAssemblySymbol)probe.GetAssemblyOrModuleSymbol(c);
		Assert.DoesNotContain(cAssembly.Modules.Single().ReferencedAssemblySymbols, x => x.Identity.Name == "Fluxor");

		(_, ImmutableArray<Diagnostic> diagnostics) = GeneratorTestHost.Run(
			new[] { "namespace Ns; public class Plain { }" },
			"true",
			referenceCBeforeB ? new[] { c, b } : new[] { b, c },
			additionalTexts: new[] { new GeneratorTestHost.InMemoryAdditionalText("Pages/Counter.razor", "@inherits C.Derived") });

		Assert.Equal(
			new[] { "Pages/Counter.razor (a Razor file that mentions Derived; the classes Razor generates are not visible to this generator)" },
			Members(diagnostics));
	}

	/// <summary>Assembly B (references Fluxor) and assembly C (references B only), as emitted images.</summary>
	internal static (Compilation b, Compilation c) CreateReferenceChain(string baseClass, string baseName)
	{
		string bSource = "using Fluxor; using System.Threading.Tasks; namespace B; public record RefState; public record RefAction; " + baseClass;
		Compilation b = GeneratorTestHost.CreateCompilation(new[] { bSource }, "Fluxor.SourceGenerator.ChainB");
		Compilation c = GeneratorTestHost.CreateCompilation(
			new[] { "namespace C; public class Derived : B." + baseName + " { }" },
			"Fluxor.SourceGenerator.ChainC",
			new[] { GeneratorTestHost.CreateReference(b) });
		return (b, c);
	}

	private static ImmutableArray<Diagnostic> Run(string source, params (string Path, string Text)[] razorFiles) =>
		Run(new[] { source }, razorFiles);

	private static ImmutableArray<Diagnostic> Run(string[] sources, params (string Path, string Text)[] razorFiles) =>
		RunWithOutput(sources, razorFiles).diagnostics;

	private static (Compilation output, ImmutableArray<Diagnostic> diagnostics) RunWithOutput(string source, params (string Path, string Text)[] razorFiles) =>
		RunWithOutput(new[] { source }, razorFiles);

	private static (Compilation output, ImmutableArray<Diagnostic> diagnostics) RunWithOutput(string[] sources, (string Path, string Text)[] razorFiles) =>
		GeneratorTestHost.Run(
			sources,
			"true",
			additionalTexts: razorFiles.Select(x => new GeneratorTestHost.InMemoryAdditionalText(x.Path, x.Text)).ToArray());

	private const string MessageSuffix = " cannot be registered by generated code; this assembly uses the reflection scan";

	private static string[] Members(ImmutableArray<Diagnostic> diagnostics) =>
		diagnostics
			.Select(x =>
			{
				Assert.Equal("FLXG001", x.Id);
				string message = x.GetMessage(CultureInfo.InvariantCulture);
				Assert.EndsWith(MessageSuffix, message);
				return message.Substring(0, message.Length - MessageSuffix.Length);
			})
			.ToArray();

	private sealed class UnreadableText : AdditionalText
	{
		public UnreadableText(string path) => Path = path;
		public override string Path { get; }
		public override SourceText GetText(CancellationToken cancellationToken = default) => null;
	}
}
