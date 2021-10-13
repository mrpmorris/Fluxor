namespace Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverGenericReducerClassesTests.SupportFiles
{
	public class DescendantGenericReducerClass : OpenGenericReducerClass<TestAction>
	{
	}

	public class OpenGenericReducerClass<TAction> : Reducer<TestState, TAction>
	{
		public override TestState Reduce(TestState state, TAction action) =>
			new TestState(count: state.Count + 1);
	}
}
