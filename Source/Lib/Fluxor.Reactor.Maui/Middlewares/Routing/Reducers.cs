namespace Fluxor.Reactor.Maui.Middlewares.Routing;

internal static class Reducers 
{
	[ReducerMethod]
	public static RoutingState ReduceGoAction(RoutingState state, GoAction action) =>
		new RoutingState(action.NewUri ?? "");

	[ReducerMethod]
	public static RoutingState ReduceGoAction<P>(RoutingState state, GoAction<P> action) where P : new() =>
		new RoutingState(action.NewUri ?? "");
}
