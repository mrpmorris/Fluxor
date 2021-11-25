using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverGenericReducersWithActionInMethodSignatureTests.SupportFiles
{
	public class TestState<T>
	{
		public ReadOnlyDictionary<T, int> Counters { get; }

		public TestState(IEnumerable<KeyValuePair<T, int>> counters)
		{
			Counters = new ReadOnlyDictionary<T, int>(counters.ToDictionary(x => x.Key, x => x.Value));
		}
	}
}
