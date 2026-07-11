namespace Fluxor.Reactor.Maui.Middlewares.Routing;

/// <summary>
/// The feature required by <see cref="RoutingMiddleware"/> to store URL state
/// </summary>
internal class Feature : Feature<RoutingState>
{
	/// <see cref="IFeature.GetName"/>
	public override string GetName() => "@routing";

	/// <summary>
	/// Provides the initial state of the routing feature
	/// </summary>
	/// <returns>State containing the current URL</returns>
	protected override RoutingState GetInitialState() => new RoutingState(string.Empty);
}
