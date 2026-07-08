using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace Fluxor.Reactor.Maui.Middlewares.Routing;

/// <summary>
/// Adds support for routing <see cref="MauiReactor.NavigationManager"/>
/// via a Fluxor store.
/// </summary>
internal class RoutingMiddleware : Middleware
{
	private readonly Shell Shell;
	private readonly IFeature<RoutingState> Feature;
	private IDispatcher Dispatcher;

	/// <summary>
	/// Creates a new instance of the routing middleware
	/// </summary>
	/// <param name="shell">Uri helper</param>
	/// <param name="feature">The routing feature</param>
	public RoutingMiddleware(Shell shell, IFeature<RoutingState> feature)
	{
		Shell = shell;
		Feature = feature;
		Shell.Navigated += Navigated;
	}

	/// <see cref="IMiddleware.InitializeAsync(IDispatcher dispatcher, IStore store)"/>
	public override Task InitializeAsync(IDispatcher dispatcher, IStore store)
	{
		Dispatcher = dispatcher;
		// If the URL changed before we initialized then dispatch an action
		Dispatcher.Dispatch(new GoAction(Shell.CurrentState.Location.AbsoluteUri));
		return Task.CompletedTask;
	}

	/// <see cref="Middleware.OnInternalMiddlewareChangeEnding"/>
	protected override void OnInternalMiddlewareChangeEnding()
	{
		string fullUri = new ShellNavigationState(Feature.State.Uri).Location.AbsoluteUri;
		if (Feature.State.Uri is not null && !UrlComparer.AreEqual(fullUri, Shell.CurrentState.Location.AbsoluteUri))
			Shell.GoToAsync(Feature.State.Uri);
	}

	private void Navigated(object sender, ShellNavigatedEventArgs e)
	{
		string fullUri = new ShellNavigationState(Feature.State.Uri).Location.AbsoluteUri;
		if (Dispatcher is not null && !IsInsideMiddlewareChange && !UrlComparer.AreEqual(e.Current.Location.AbsoluteUri, fullUri))
			Dispatcher.Dispatch(new GoAction(e.Current.Location.AbsoluteUri));
	}
}
