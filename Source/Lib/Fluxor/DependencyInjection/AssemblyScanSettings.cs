using Fluxor.DependencyInjection.Generated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fluxor.DependencyInjection;

internal class AssemblyScanSettings
{
	public readonly Assembly Assembly;
	public readonly string Namespace;

	public AssemblyScanSettings(Assembly assembly) : this(assembly, null) { }

	public AssemblyScanSettings(Assembly assembly, string @namespace)
	{
		Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
		Namespace = @namespace;
	}

	public bool Matches(Type type) =>
		type.Assembly == Assembly
		&&
		(
			Namespace is null
			|| type.FullName.StartsWith(Namespace + ".", StringComparison.InvariantCultureIgnoreCase)
		);

	public static Type[] FilterClasses(
		IEnumerable<Type> types,
		IEnumerable<AssemblyScanSettings> scanExcludeList,
		IEnumerable<AssemblyScanSettings> scanIncludeList)
		=> types
			.Where(t =>
				scanIncludeList.Any(incl => incl.Matches(t))
				|| !scanExcludeList.Any(excl => excl.Matches(t)))
			.ToArray();

	public static TypeAndMethodInfo[] FilterMethods(
		IEnumerable<Type> allCandidateTypes,
		GeneratedRegistrationCatalog catalog) =>
		allCandidateTypes
			.SelectMany(t =>
				catalog.TryGetEntry(t, out GeneratedTypeEntry entry)
				? GetGeneratedMethods(t, entry)
				: GetReflectedMethods(t))
			.ToArray();

	// Reducer and effect methods are later filtered into separate lists, so only the order within
	// each kind matters; the generated entry lists each kind in GetMethods order.
	private static IEnumerable<TypeAndMethodInfo> GetGeneratedMethods(Type type, GeneratedTypeEntry entry) =>
		entry.ReducerMethods
			.Select(m => new TypeAndMethodInfo(type, m))
			.Concat(entry.EffectMethods
				.Select(m => new TypeAndMethodInfo(type, m)));

	private static IEnumerable<TypeAndMethodInfo> GetReflectedMethods(Type type) =>
		type
			.GetMethods(
				BindingFlags.Public
				| BindingFlags.Instance
				| BindingFlags.Static
				| BindingFlags.FlattenHierarchy)
			.Where(m =>
				m.GetCustomAttributes(true).Any(a => a is ReducerMethodAttribute || a is EffectMethodAttribute))
			.Select(m => new TypeAndMethodInfo(type, m));

	public override bool Equals(object obj)
	{
		var other = obj as AssemblyScanSettings;
		if (other is null)
			return false;

		return
			other.Assembly.FullName == Assembly.FullName
			&& other.Namespace is not null
			&& other.Namespace.Equals(Namespace, StringComparison.InvariantCultureIgnoreCase);
	}

	public override int GetHashCode()
	{
		return (Assembly.FullName + "/" + Namespace).GetHashCode();
	}
}
