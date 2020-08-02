using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fluxor.DependencyInjection
{
	internal class AssemblyScanSettings
	{
		public readonly Assembly Assembly;
		public readonly string Namespace;

		public AssemblyScanSettings(Assembly assembly) : this(assembly, null) { }
		public bool Matches(Type type) =>
			type.Assembly == Assembly
			&& (Namespace == null || type.FullName.StartsWith(Namespace + "."));


		public AssemblyScanSettings(Assembly assembly, string @namespace)
		{
			Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
			Namespace = @namespace;
		}

		public static IEnumerable<Type> FilterClasses(IEnumerable<Type> types, IEnumerable<AssemblyScanSettings> scanExcludeList, 
			IEnumerable<AssemblyScanSettings> scanIncludeList)
			=> types
				.Where(t =>
					scanIncludeList.Any(wl => wl.Matches(t))
					|| !scanExcludeList.Any(bl => bl.Matches(t)))
				.ToArray();

		public static IEnumerable<MethodInfo> FilterMethods(IEnumerable<Type> allCandidateTypes) =>
			allCandidateTypes
			.SelectMany(t =>
				t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.NonPublic))
			.Where(m =>
				CustomAttributeExtensions.GetCustomAttribute(m, typeof(EffectMethodAttribute)) != null
				|| CustomAttributeExtensions.GetCustomAttribute(m, typeof(ReducerMethodAttribute)) != null)
			.ToArray();

		public override bool Equals(object obj)
		{
			AssemblyScanSettings other = obj as AssemblyScanSettings;
			if (other == null)
				return false;

			return other.Assembly.FullName == Assembly.FullName && other.Namespace == Namespace;
		}

		public override int GetHashCode()
		{
			return (Assembly.FullName + "/" + Namespace).GetHashCode();
		}
	}
}
