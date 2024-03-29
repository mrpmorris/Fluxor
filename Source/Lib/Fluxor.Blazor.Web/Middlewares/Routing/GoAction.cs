﻿namespace Fluxor.Blazor.Web.Middlewares.Routing;

/// <summary>
/// Dispatching this action will navigate the browser to the specified URL
/// </summary>
/// <seealso cref="Microsoft.AspNetCore.Components.NavigationManager"/>
public class GoAction
{
	/// <summary>
	/// The new address to navigate to
	/// </summary>
	public string NewUri { get; }

	/// <summary>
	/// When true forces a real browser navigation and page reload
	/// </summary>
	public bool ForceLoad { get; }

	/// <summary>
	/// Creates a new instance of the action
	/// </summary>
	/// <param name="newUri"></param>
	public GoAction(string newUri, bool forceLoad = false)
	{
		NewUri = newUri;
		ForceLoad = forceLoad;
	}
}
