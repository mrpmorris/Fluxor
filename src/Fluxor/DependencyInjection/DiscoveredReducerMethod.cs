using System;
using System.Reflection;

namespace Fluxor.DependencyInjection
{
	internal class DiscoveredReducerMethod
	{
		public readonly Type HostClassType;
		public readonly MethodInfo MethodInfo;
		public readonly Type StateType;
		public readonly Type ActionType;

		public DiscoveredReducerMethod(Type hostClassType, MethodInfo methodInfo, Type stateType, Type actionType)
		{
			HostClassType = hostClassType;
			MethodInfo = methodInfo;
			StateType = stateType;
			ActionType = actionType;
		}
	}
}
