namespace Fluxor.UnitTests.ActionSubscriberTests.SubscribeToActionTests.SupportFiles
{
	public static class Reducers
	{
		[ReducerMethod(typeof(TestAction))]
		public static State Reduce(State state) => state with { DispatchCount = state.DispatchCount + 1 };
	}
}
