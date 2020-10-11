using System;
using System.Linq;

namespace Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverGenericReducersWithActionInMethodSignatureTests.SupportFiles
{
	public class TestIntReducer: AbstractTestReducers<int>
	{

	}

	public abstract class AbstractTestReducers<T>
		where T : IEquatable<T>
	{
		[ReducerMethod]
		public static TestState<T> ReduceTestAction(TestState<T> state, RemoveItemAction<T> action) =>
			new TestState<T>(state.Items.Where(x => !x.Equals(action.Item)).ToArray());
	}
}
