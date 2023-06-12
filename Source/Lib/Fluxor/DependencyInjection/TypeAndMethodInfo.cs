using System;
using System.Reflection;

namespace Fluxor.DependencyInjection
{
	internal struct TypeAndMethodInfo
	{
		public readonly Type Type;
		public readonly MethodInfo MethodInfo;
		public readonly Attribute MethodAttribute;

		public TypeAndMethodInfo(Type type, MethodInfo methodInfo, Attribute methodAttribute)
		{
			Type = type;
			MethodInfo = methodInfo;
			MethodAttribute = methodAttribute;
		}
	}
}
