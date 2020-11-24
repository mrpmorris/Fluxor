using System;
using System.Reflection;

namespace Fluxor.Extensions
{
	internal static class MethodInfoExtentions
	{
		public static string GetClassNameAndMethodName(this MethodInfo methodInfo)
		{
			if (methodInfo == null)
				throw new ArgumentNullException(nameof(methodInfo));

			return $"Method \"{methodInfo.Name}\" on class \"{methodInfo.DeclaringType.FullName}\"";
		}
	}
}
