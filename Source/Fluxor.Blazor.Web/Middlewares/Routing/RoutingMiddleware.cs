using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fluxor.Blazor.Web.Middlewares.Routing
{
	/// <summary>
	/// Adds support for routing <see cref="Microsoft.AspNetCore.Components.NavigationManager"/>
	/// via a Fluxor store.
	/// </summary>
	internal class RoutingMiddleware : Middleware
	{
		private readonly TimeSpan LoopRedirectDetectionWindow;
		private readonly NavigationManager NavigationManager;
		private readonly IFeature<RoutingState> Feature;
		private IStore Store;
		private (string Url, DateTime NavigationTime)[] PreviousNavigations;

		/// <summary>
		/// Creates a new instance of the routing middleware
		/// </summary>
		/// <param name="navigationManager">Uri helper</param>
		/// <param name="feature">The routing feature</param>
		public RoutingMiddleware(NavigationManager navigationManager, IFeature<RoutingState> feature)
		{
			NavigationManager = navigationManager;
			Feature = feature;
			LoopRedirectDetectionWindow = TimeSpan.FromMilliseconds(100);
			PreviousNavigations = Array.Empty<(string Url, DateTime NavigationTime)>();
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
			if (Store != null
				&& !IsInsideMiddlewareChange
				&& e.Location != Feature.State.Uri
				&& !LoopedRedirectDetected(e))
			{
				Store.Dispatch(new GoAction(e.Location));
			}
		}

		private bool LoopedRedirectDetected(LocationChangedEventArgs e)
		{
			if (e.IsNavigationIntercepted)
				return false;

			DateTime cutoffTime = DateTime.UtcNow.Subtract(LoopRedirectDetectionWindow);
			PreviousNavigations =
				PreviousNavigations
				.Where(x => x.NavigationTime >= cutoffTime)
				.Append((e.Location, DateTime.UtcNow))
				.ToArray();

			return (PreviousNavigations.Count(x => x.Url == e.Location) >= 2);
		}
	}
}
