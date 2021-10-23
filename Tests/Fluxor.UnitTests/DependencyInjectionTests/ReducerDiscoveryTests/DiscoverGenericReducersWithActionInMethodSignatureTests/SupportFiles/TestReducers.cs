using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverGenericReducersWithActionInMethodSignatureTests.SupportFiles
{
	public class DescendantGenericReducers: OpenGenericReducers<char>
	{
	}

	public class OpenGenericReducers<T>
		where T : IEquatable<T>
	{
		[ReducerMethod]
		public static TestState<T> ReduceRemoveItemAction(TestState<T> state, IncrementItemAction<T> action) =>
			new TestState<T>(
				state.Counters.Select(x =>
					!x.Key.Equals(action.Item)
					? x
					: new KeyValuePair<T, int>(x.Key, x.Value + 1)));
	}
}
