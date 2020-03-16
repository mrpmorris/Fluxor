using System;
using System.Reflection;

namespace Fluxor.DependencyInjection
{
	internal class DiscoveredEffectMethod
	{
		public readonly Type HostClassType;
		public readonly MethodInfo MethodInfo;
		public readonly Type ActionType;

		public DiscoveredEffectMethod(Type hostClassType, MethodInfo methodInfo, Type actionType)
		{
			HostClassType = hostClassType;
			MethodInfo = methodInfo;
			ActionType = actionType;
		}
	}
}
