using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;

namespace Fluxor.SourceGenerator.UnitTests;

/// <summary>
/// Compiles test sources in memory, runs <see cref="FluxorRegistrationGenerator"/> over them and loads the result.
/// </summary>
public static class GeneratorTestHost
{
	public const string AssemblyName = "Fluxor.SourceGenerator.TestAssembly";
	public const string OptInPropertyKey = "build_property.FluxorGenerateRegistration";

	private static readonly Lazy<ImmutableArray<MetadataReference>> PlatformReferences = new(() => CreatePlatformReferences(includeFluxor: true));
	private static readonly Lazy<ImmutableArray<MetadataReference>> FrameworkReferences = new(() => CreatePlatformReferences(includeFluxor: false));

	public static (Compilation output, ImmutableArray<Diagnostic> diagnostics) Run(string[] sources, bool optIn) =>
		Run(sources, optIn ? "true" : null);

	/// <summary>
	/// Runs the generator with the given value of the FluxorGenerateRegistration property (null = not set).
	/// </summary>
	public static (Compilation output, ImmutableArray<Diagnostic> diagnostics) Run(
		string[] sources,
		string optInValue,
		IEnumerable<MetadataReference> additionalReferences = null,
		string assemblyName = AssemblyName,
		IEnumerable<AdditionalText> additionalTexts = null,
		bool referenceFluxor = true)
	{
		CSharpCompilation compilation = CreateCompilation(sources, assemblyName, additionalReferences, referenceFluxor);
		var properties = new Dictionary<string, string>();
		if (optInValue is not null)
			properties[OptInPropertyKey] = optInValue;

		GeneratorDriver driver = CSharpGeneratorDriver.Create(
			generators: new[] { new FluxorRegistrationGenerator().AsSourceGenerator() },
			additionalTexts: additionalTexts,
			parseOptions: (CSharpParseOptions)compilation.SyntaxTrees.First().Options,
			optionsProvider: new TestAnalyzerConfigOptionsProvider(properties));

		driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out ImmutableArray<Diagnostic> diagnostics);
		return (output, diagnostics);
	}

	/// <summary>
	/// Creates a compilation of the sources against the running runtime and (unless
	/// <paramref name="referenceFluxor"/> is false) Fluxor.
	/// </summary>
	public static CSharpCompilation CreateCompilation(
		string[] sources,
		string assemblyName = AssemblyName,
		IEnumerable<MetadataReference> additionalReferences = null,
		bool referenceFluxor = true)
	{
		var parseOptions = new CSharpParseOptions(LanguageVersion.Latest);
		return CSharpCompilation.Create(
			assemblyName,
			sources.Select((source, index) => CSharpSyntaxTree.ParseText(source, parseOptions, path: $"Source{index}.cs")),
			(referenceFluxor ? PlatformReferences.Value : FrameworkReferences.Value)
				.AddRange(additionalReferences ?? Array.Empty<MetadataReference>()),
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Disable));
	}

	/// <summary>
	/// Emits the compilation and loads it into a collectible <see cref="AssemblyLoadContext"/>. Fluxor and the
	/// framework come from the default context; <paramref name="dependencies"/> are emitted and loaded alongside.
	/// </summary>
	public static Assembly LoadAssembly(Compilation compilation, params Compilation[] dependencies)
	{
		var context = new AssemblyLoadContext($"Fluxor generator test {Guid.NewGuid()}", isCollectible: true);
		foreach (Compilation dependency in dependencies)
			context.LoadFromStream(new MemoryStream(Emit(dependency)));
		return context.LoadFromStream(new MemoryStream(Emit(compilation)));
	}

	/// <summary>
	/// Emits the compilation to a reference another in-memory compilation can use.
	/// </summary>
	public static MetadataReference CreateReference(Compilation compilation) =>
		MetadataReference.CreateFromImage(Emit(compilation));

	private static byte[] Emit(Compilation compilation)
	{
		using var stream = new MemoryStream();
		var result = compilation.Emit(stream);
		if (!result.Success)
			throw new InvalidOperationException(
				"Compilation failed:" + Environment.NewLine
				+ string.Join(Environment.NewLine, result.Diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error)));
		return stream.ToArray();
	}

	private static ImmutableArray<MetadataReference> CreatePlatformReferences(bool includeFluxor)
	{
		string fluxorLocation = typeof(IFeature).Assembly.Location;
		string trustedPlatformAssemblies = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
		return trustedPlatformAssemblies
			.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
			.Append(fluxorLocation)
			.Where(path => includeFluxor || !string.Equals(path, fluxorLocation, StringComparison.OrdinalIgnoreCase))
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.Select(path => (MetadataReference)MetadataReference.CreateFromFile(path))
			.ToImmutableArray();
	}

	/// <summary>
	/// An additional file (e.g. a .razor file) passed to the generator.
	/// </summary>
	public sealed class InMemoryAdditionalText : AdditionalText
	{
		private readonly SourceText Text;

		public InMemoryAdditionalText(string path, string text)
		{
			Path = path;
			Text = SourceText.From(text);
		}

		public override string Path { get; }
		public override SourceText GetText(CancellationToken cancellationToken = default) => Text;
	}

	private sealed class TestAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
	{
		private readonly TestAnalyzerConfigOptions Options;

		public TestAnalyzerConfigOptionsProvider(Dictionary<string, string> globalOptions)
		{
			Options = new TestAnalyzerConfigOptions(globalOptions);
		}

		public override AnalyzerConfigOptions GlobalOptions => Options;
		public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => TestAnalyzerConfigOptions.Empty;
		public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => TestAnalyzerConfigOptions.Empty;
	}

	private sealed class TestAnalyzerConfigOptions : AnalyzerConfigOptions
	{
		public static readonly TestAnalyzerConfigOptions Empty = new(new Dictionary<string, string>());

		private readonly Dictionary<string, string> Values;

		public TestAnalyzerConfigOptions(Dictionary<string, string> values)
		{
			Values = values;
		}

		public override bool TryGetValue(string key, out string value) => Values.TryGetValue(key, out value);
	}
}
