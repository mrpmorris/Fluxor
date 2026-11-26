using System.ComponentModel;

namespace Fluxor.DependencyInjection.Generated;

/// <summary>
/// Describes the format version of generated registration code.
/// </summary>
/// <remarks>
/// This Fluxor reads only <see cref="Current"/>; an assembly marked with any other version is scanned by
/// reflection. Version 1 descriptors carry a shared factory delegate plus an index (see
/// <see cref="GeneratedReducerMethod.Index"/>), do not describe action types, and may be built from a table by
/// <see cref="GeneratedRegistrationTable"/>.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class FluxorGeneratedRegistrationFormat
{
	/// <summary>
	/// The format version understood by this version of Fluxor (the only one it reads).
	/// </summary>
	public const int Current = 1;
}
