using Fluxor.DependencyInjection.Generated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fluxor.DependencyInjection;

/// <summary>
/// The generated registrations of the assemblies taking part in one <c>AddFluxor</c> call.
/// An assembly is in the catalog only when it carries a <see cref="FluxorGeneratedRegistrationAttribute"/>
/// with a format version this Fluxor understands; every other assembly is scanned by reflection.
/// </summary>
internal sealed class GeneratedRegistrationCatalog
{
	internal const string DisableSwitchName = "Fluxor.DisableGeneratedRegistration";

	public static readonly GeneratedRegistrationCatalog Empty = new(
		new Dictionary<Assembly, Type[]>(),
		new Dictionary<Type, GeneratedTypeEntry>());

	private readonly Dictionary<Assembly, Type[]> TypesByAssembly;
	private readonly Dictionary<Type, GeneratedTypeEntry> EntriesByType;

	private GeneratedRegistrationCatalog(
		Dictionary<Assembly, Type[]> typesByAssembly,
		Dictionary<Type, GeneratedTypeEntry> entriesByType)
	{
		TypesByAssembly = typesByAssembly;
		EntriesByType = entriesByType;
	}

	/// <summary>
	/// Builds the catalog for the given assemblies. Returns an empty catalog when the AppContext switch
	/// <c>Fluxor.DisableGeneratedRegistration</c> is on.
	/// </summary>
	public static GeneratedRegistrationCatalog Create(IEnumerable<Assembly> assemblies) =>
		Create(assemblies, AppContext.TryGetSwitch);

	/// <summary>
	/// Reads an AppContext switch the way <see cref="AppContext.TryGetSwitch"/> does.
	/// </summary>
	internal delegate bool SwitchReader(string switchName, out bool isEnabled);

	/// <summary>
	/// Builds the catalog, reading the <c>Fluxor.DisableGeneratedRegistration</c> switch with
	/// <paramref name="tryGetSwitch"/> (tests pass a reader of their own).
	/// </summary>
	internal static GeneratedRegistrationCatalog Create(IEnumerable<Assembly> assemblies, SwitchReader tryGetSwitch)
	{
		if (tryGetSwitch(DisableSwitchName, out bool isDisabled) && isDisabled)
			return Empty;

		var typesByAssembly = new Dictionary<Assembly, Type[]>();
		var entriesByType = new Dictionary<Type, GeneratedTypeEntry>();
		foreach (Assembly assembly in assemblies.Distinct())
		{
			IFluxorGeneratedRegistration registration = GetRegistration(assembly);
			if (registration is null)
				continue;

			var entries = new List<GeneratedTypeEntry>();
			foreach (GeneratedTypeEntry entry in registration.Types)
			{
				// A registration only describes its own assembly
				if (entry?.Type is null || entry.Type.Assembly != assembly)
					continue;
				if (entriesByType.TryAdd(entry.Type, entry))
					entries.Add(entry);
			}

			// Metadata token order is the order Assembly.GetTypes() returns types in
			typesByAssembly[assembly] = entries
				.Select(x => x.Type)
				.OrderBy(x => x.MetadataToken)
				.ToArray();
		}

		if (typesByAssembly.Count == 0)
			return Empty;

		return new GeneratedRegistrationCatalog(typesByAssembly, entriesByType);
	}

	/// <summary>
	/// Gets the Fluxor types of an assembly that has a generated registration, in metadata order.
	/// </summary>
	public bool TryGetTypes(Assembly assembly, out Type[] typesInMetadataOrder) =>
		TypesByAssembly.TryGetValue(assembly, out typesInMetadataOrder);

	/// <summary>
	/// Gets the generated description of a type.
	/// </summary>
	public bool TryGetEntry(Type type, out GeneratedTypeEntry entry) =>
		EntriesByType.TryGetValue(type, out entry);

	private static IFluxorGeneratedRegistration GetRegistration(Assembly assembly)
	{
		FluxorGeneratedRegistrationAttribute attribute =
			assembly.GetCustomAttribute<FluxorGeneratedRegistrationAttribute>();

		if (attribute is null)
			return null;

		// Unknown (e.g. newer) formats are ignored silently: the assembly is scanned by reflection
		if (attribute.FormatVersion != FluxorGeneratedRegistrationFormat.Current)
			return null;

		Type registrationType = attribute.RegistrationType;
		if (registrationType is null || !typeof(IFluxorGeneratedRegistration).IsAssignableFrom(registrationType))
			return null;

		return (IFluxorGeneratedRegistration)Activator.CreateInstance(registrationType, nonPublic: true);
	}
}
