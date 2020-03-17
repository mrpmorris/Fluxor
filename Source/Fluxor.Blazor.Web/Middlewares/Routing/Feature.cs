using Microsoft.AspNetCore.Components;

namespace Fluxor.Blazor.Web.Middlewares.Routing
{
	/// <summary>
	/// The feature required by <see cref="RoutingMiddleware"/> to store URL state
	/// </summary>
	internal class Feature : Feature<RoutingState>
	{
		private readonly string InitialUrl;

		/// <see cref="IFeature.GetName"/>
		public override string GetName() => "@routing";

		/// <summary>
		/// Provides the initial state of the routing feature
		/// </summary>
		/// <returns>State containing the current URL</returns>
		protected override RoutingState GetInitialState() => new RoutingState(InitialUrl);

		/// <summary>
		/// Creates a new instance of the routing feature
		/// </summary>
		/// <param name="navigationManager">Uri helper</param>
		public Feature(NavigationManager navigationManager)
		{
			InitialUrl = navigationManager.Uri;
		}
	}
}
