using System.Collections.Generic;
using System.ComponentModel;

namespace Fluxor.DependencyInjection.Generated;

/// <summary>
/// Implemented by generated code to describe the Fluxor-relevant types of an assembly.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IFluxorGeneratedRegistration
{
	/// <summary>
	/// Fluxor-relevant types of this assembly only (no generic type definitions), in any order.
	/// </summary>
	IReadOnlyList<GeneratedTypeEntry> Types { get; }
}
