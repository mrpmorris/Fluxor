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
				Namespace is null
				|| type.FullName.StartsWith(Namespace + ".", StringComparison.InvariantCultureIgnoreCase)
			);

		public static Type[] FilterClasses(
			IEnumerable<Type> types,
			IEnumerable<AssemblyAndNamespace> scanExcludeList,
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
					})
				.SelectMany(x => x.Methods
					.Select(m =>
						new
						{
							Type = x.Type,
							MethodInfo = m,
							MethodAttribute = m
								.GetCustomAttributes(true)
								.FirstOrDefault(a => a is ReducerMethodAttribute || a is EffectMethodAttribute)
							}
					)
				.Where(x => x.MethodAttribute is not null)
				.Select(x =>
					new TypeAndMethodInfo(
						type: x.Type,
						methodInfo: x.MethodInfo,
						methodAttribute: (Attribute)x.MethodAttribute)))
				.ToArray();

		public override bool Equals(object obj)
		{
			var other = obj as AssemblyScanSettings;
			if (other is null)
				return false;

			return
				other.Assembly.FullName == Assembly.FullName
				&& string.Equals(Namespace, other.Namespace, StringComparison.InvariantCultureIgnoreCase);
		}

		public override int GetHashCode()
		{
			return (Assembly.FullName + "/" + Namespace).GetHashCode();
		}
	}
}
