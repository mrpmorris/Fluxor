using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fluxor.DependencyInjection
{
	internal class AssemblyAndNamespace
	{
		public readonly Assembly Assembly;
		public readonly string Namespace;

		public AssemblyAndNamespace(Assembly assembly) : this(assembly, null) { }

		public AssemblyAndNamespace(Assembly assembly, string @namespace)
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

		public override bool Equals(object obj)
		{
			var other = obj as AssemblyAndNamespace;
			if (other is null)
				return false;

			return
				other.Assembly.FullName == Assembly.FullName
				&& string.Equals(Namespace, other.Namespace, StringComparison.InvariantCultureIgnoreCase) == true;
		}

		public override int GetHashCode()
		{
			return (Assembly.FullName + "/" + Namespace).GetHashCode();
		}
	}
}
