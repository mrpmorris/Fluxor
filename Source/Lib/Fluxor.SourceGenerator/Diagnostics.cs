using Microsoft.CodeAnalysis;

namespace Fluxor.SourceGenerator;

internal static class Diagnostics
{
	public static readonly DiagnosticDescriptor CannotGenerateRegistration = new(
		id: "FLXG001",
		title: "Fluxor registration cannot be generated",
		messageFormat: "{0} cannot be registered by generated code; this assembly uses the reflection scan",
		category: "Fluxor",
		defaultSeverity: DiagnosticSeverity.Warning,
		isEnabledByDefault: true,
		description:
			"FluxorGenerateRegistration is on, but a member cannot be registered by generated code exactly as the"
			+ " reflection scan registers it. No registration is generated for the assembly, so Fluxor scans it by"
			+ " reflection as usual. Fix the member, or suppress the warning to keep the reflection scan.");
}
