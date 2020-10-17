using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverGenericReducersWithActionInMethodSignatureTests.SupportFiles
{
	public class TestState<T>
	{
		public T[] Items;

		public TestState(IEnumerable<T> items)
		{
			Items = items.ToArray();
		}
	}
}
