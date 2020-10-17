namespace Fluxor.UnitTests.DependencyInjectionTests.EffectDiscoveryTests.DiscoverEffectsWithActionInMethodSignatureTests.SupportFiles
{
	public static class Reducers
	{
		[ReducerMethod(typeof(EffectDispatchedAction))]
		public static TestState Reduce(TestState state) =>
			new TestState(state.Count + 1);
	}
}
