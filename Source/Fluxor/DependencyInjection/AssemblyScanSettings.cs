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

		public AssemblyScanSettings(Assembly assembly, string @namespace)
		{
			Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
			Namespace = @namespace;
		}

		public bool Matches(Type type) =>
				type.Assembly == Assembly
				&&
				(
					Namespace == null
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

		public static TypeAndMethodInfo[] FilterMethods(IEnumerable<Type> allCandidateTypes) =>
			allCandidateTypes
				.Select(t =>
					new
					{
						Type = t,
						Methods = t
							.GetMethods(
								BindingFlags.Public
								| BindingFlags.Instance
								| BindingFlags.Static
								| BindingFlags.FlattenHierarchy)
							.Where(m =>
								m.GetCustomAttributes(true).Any(a => a is ReducerMethodAttribute || a is EffectMethodAttribute))
					})
				.SelectMany(x => x.Methods
					.Select(m => new TypeAndMethodInfo(x.Type, m)))
				.ToArray();

		public override bool Equals(object obj)
		{
			AssemblyScanSettings other = obj as AssemblyScanSettings;
			if (other == null)
				return false;

			return
				other.Assembly.FullName == Assembly.FullName
				&& other.Namespace != null
				&& other.Namespace.Equals(Namespace, StringComparison.InvariantCultureIgnoreCase);
		}

		public override int GetHashCode()
		{
			return (Assembly.FullName + "/" + Namespace).GetHashCode();
		}
	}
}
