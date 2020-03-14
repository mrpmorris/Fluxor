using System;

namespace Fluxor
{
	internal static class TypeHelper
	{
		internal static Type[] GetGenericParametersForImplementedInterface(Type implementingType, Type genericInterfaceRequired)
		{
			foreach(Type interfaceType in implementingType.GetInterfaces())
			{
				if (!interfaceType.IsGenericType)
					continue;

				Type genericTypeForInterface = interfaceType.GetGenericTypeDefinition();
				if (genericTypeForInterface == genericInterfaceRequired)
					return interfaceType.GetGenericArguments();
			}
			return null;
		}
	}
}
