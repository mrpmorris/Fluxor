using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using System.Threading.Tasks;

namespace Fluxor.Blazor.Web.Middlewares.Routing
{
	/// <summary>
	/// Adds support for routing <see cref="Microsoft.AspNetCore.Components.NavigationManager"/>
	/// via a Fluxor store.
	/// </summary>
	internal class RoutingMiddleware : Middleware
	{
		private readonly NavigationManager NavigationManager;
		private readonly IFeature<RoutingState> Feature;
		private IStore Store;

		/// <summary>
		/// Creates a new instance of the routing middleware
		/// </summary>
		/// <param name="navigationManager">Uri helper</param>
		/// <param name="feature">The routing feature</param>
		public RoutingMiddleware(NavigationManager navigationManager, IFeature<RoutingState> feature)
		{
			NavigationManager = navigationManager;
			Feature = feature;
			NavigationManager.LocationChanged += LocationChanged;
		}

		/// <see cref="IMiddleware.InitializeAsync(IStore)"/>
		public override Task InitializeAsync(IStore store)
		{
			Store = store;
			// If the URL changed before we initialized then dispatch an action
			Store.Dispatch(new GoAction(NavigationManager.Uri));
			return Task.CompletedTask;
		}

		/// <see cref="Middleware.OnInternalMiddlewareChangeEnding"/>
		protected override void OnInternalMiddlewareChangeEnding()
		{
			if (Feature.State.Uri != NavigationManager.Uri && Feature.State.Uri != null)
				NavigationManager.NavigateTo(Feature.State.Uri);
		}

		private void LocationChanged(object sender, LocationChangedEventArgs e)
		{
			if (Store != null && !IsInsideMiddlewareChange && e.Location != Feature.State.Uri)
				Store.Dispatch(new GoAction(e.Location));
		}
	}
}
