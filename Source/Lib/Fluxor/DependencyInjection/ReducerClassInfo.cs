using System;

namespace Fluxor.DependencyInjection
{
	internal class ReducerClassInfo
	{
		public readonly Type ImplementingType;
		public readonly Type StateType;

		public ReducerClassInfo(Type implementingType, Type stateType)
		{
			ImplementingType = implementingType;
			StateType = stateType;
		}

		internal object GroupBy(Func<object, object> p)
		{
			throw new NotImplementedException();
		}
	}
}
