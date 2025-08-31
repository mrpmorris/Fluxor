using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using System;
using System.Threading.Tasks;

namespace Fluxor.Blazor.Web.Middlewares.Routing;

/// <summary>
/// Adds support for routing <see cref="Microsoft.AspNetCore.Components.NavigationManager"/>
/// via a Fluxor store.
/// </summary>
internal class RoutingMiddleware : Middleware
{
	private readonly NavigationManager NavigationManager;
	private readonly IFeature<RoutingState> Feature;
	private IDispatcher Dispatcher;
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
	public override Task InitializeAsync(IDispatcher dispatcher, IStore store)
	{
		Dispatcher = dispatcher;
		Store = store;
		// If the URL changed before we initialized then dispatch an action
		Console.WriteLine($"Routing: WasPersisted = {Store.WasPersisted}");
		if (!store.WasPersisted)
			Dispatcher.Dispatch(new GoAction(NavigationManager.Uri));
		return Task.CompletedTask;
	}

	/// <see cref="Middleware.OnInternalMiddlewareChangeEnding"/>
	protected override void OnInternalMiddlewareChangeEnding()
	{
		string fullUri = NavigationManager.ToAbsoluteUri(Feature.State.Uri).AbsoluteUri;
		if (Feature.State.Uri is not null && !UrlComparer.AreEqual(fullUri, NavigationManager.Uri))
			NavigationManager.NavigateTo(Feature.State.Uri);
	}

	private void LocationChanged(object sender, LocationChangedEventArgs e)
	{
		string fullUri = NavigationManager.ToAbsoluteUri(Feature.State.Uri).AbsoluteUri;
		if (Dispatcher is not null && !IsInsideMiddlewareChange && !UrlComparer.AreEqual(e.Location, fullUri))
			Dispatcher.Dispatch(new GoAction(e.Location));
	}
}
