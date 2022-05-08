namespace Fluxor.Blazor.Web.Middlewares.Routing
{
	internal static class Reducers 
	{
		[ReducerMethod]
		public static RoutingState ReduceGoAction(RoutingState state, GoAction action) =>
			new RoutingState(action.NewUri ?? "");
	}
}
