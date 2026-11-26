using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Fluxor.SourceGenerator;

/// <summary>
/// When the project sets <c>FluxorGenerateRegistration</c> to <c>true</c>, generates the Fluxor registration of
/// the assembly (an <c>IFluxorGeneratedRegistration</c> and the assembly attribute pointing to it), so Fluxor
/// does not scan the assembly by reflection. When anything cannot be generated exactly, reports FLXG001 and
/// generates nothing: the assembly then keeps the reflection scan.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class FluxorRegistrationGenerator : IIncrementalGenerator
{
	private const string OptInProperty = "build_property.FluxorGenerateRegistration";

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		IncrementalValueProvider<bool> isOptedIn = context.AnalyzerConfigOptionsProvider
			.Select((options, _) =>
				options.GlobalOptions.TryGetValue(OptInProperty, out string? value)
				&& string.Equals(value?.Trim(), "true", StringComparison.OrdinalIgnoreCase));

		IncrementalValueProvider<ImmutableArray<AdditionalText>> razorFiles = context.AdditionalTextsProvider
			.Where(RegistrationModelBuilder.IsRazorFile)
			.Collect();

		context.RegisterSourceOutput(
			context.CompilationProvider.Combine(isOptedIn).Combine(razorFiles),
			(sourceContext, input) => Execute(sourceContext, input.Left.Left, input.Left.Right, input.Right));
	}

	private static void Execute(
		SourceProductionContext context,
		Compilation compilation,
		bool isOptedIn,
		ImmutableArray<AdditionalText> razorFiles)
	{
		if (!isOptedIn || !FluxorSymbols.ReferencesFluxor(compilation))
			return;

		FluxorSymbols? symbols = FluxorSymbols.TryCreate(compilation);
		if (symbols is null)
		{
			context.ReportDiagnostic(Diagnostic.Create(
				Diagnostics.CannotGenerateRegistration,
				Location.None,
				"This assembly (the referenced Fluxor does not support generated registration)"));
			return;
		}

		RegistrationModel model = RegistrationModelBuilder.Build(compilation, symbols, razorFiles, context.CancellationToken);
		if (model.Problems.Count > 0)
		{
			foreach (RegistrationProblem problem in model.Problems)
				context.ReportDiagnostic(Diagnostic.Create(Diagnostics.CannotGenerateRegistration, problem.Location, problem.Member));
			return;
		}

		context.AddSource(RegistrationEmitter.HintName, RegistrationEmitter.Emit(model));
	}
}
