using System;
using System.ComponentModel;

namespace Fluxor.DependencyInjection.Generated;

/// <summary>
/// Marks an assembly as containing build-time generated Fluxor registration code.
/// Intended for use by generated code only.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class FluxorGeneratedRegistrationAttribute : Attribute
{
	/// <summary>
	/// The type implementing <see cref="IFluxorGeneratedRegistration"/>.
	/// </summary>
	public Type RegistrationType { get; }

	/// <summary>
	/// The version of the generated format. See <see cref="FluxorGeneratedRegistrationFormat.Current"/>.
	/// </summary>
	public int FormatVersion { get; }

	/// <summary>
	/// Creates a new instance.
	/// </summary>
	/// <param name="registrationType">The type implementing <see cref="IFluxorGeneratedRegistration"/></param>
	/// <param name="formatVersion">The version of the generated format</param>
	public FluxorGeneratedRegistrationAttribute(Type registrationType, int formatVersion)
	{
		RegistrationType = registrationType;
		FormatVersion = formatVersion;
	}
}
