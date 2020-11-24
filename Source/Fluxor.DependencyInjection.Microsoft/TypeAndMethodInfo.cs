using System;
using System.Reflection;

namespace Fluxor.DependencyInjection.Microsoft
{
	internal struct TypeAndMethodInfo
	{
		public readonly Type Type;
		public readonly MethodInfo MethodInfo;

		public TypeAndMethodInfo(Type type, MethodInfo methodInfo)
		{
			Type = type ?? throw new ArgumentNullException(nameof(type));
			MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
		}
	}
}
