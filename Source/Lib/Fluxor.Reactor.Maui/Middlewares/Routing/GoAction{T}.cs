using System;

namespace Fluxor.Reactor.Maui.Middlewares.Routing;

/// <summary>
/// Dispatching this action will navigate the browser to the specified URL
/// </summary>
/// <seealso cref="MauiReactor.NavigationManager"/>
public class GoAction<P> where P : new()
{
	/// <summary>
	/// The new address to navigate to
	/// </summary>
	public string NewUri { get; }

	/// <summary>
	/// Props initializer callback
	/// </summary>
	public Action<P> PropsInitializer { get; }

	/// <summary>
	/// When true forces a real browser navigation and page reload
	/// </summary>
	public bool ForceLoad { get; }

	/// <summary>
	/// Creates a new instance of the action
	/// </summary>
	/// <param name="newUri"></param>
	public GoAction(string newUri, Action<P> propsInitializer, bool forceLoad = false)
	{
		NewUri = newUri;
		PropsInitializer = propsInitializer;
		ForceLoad = forceLoad;
	}
}
