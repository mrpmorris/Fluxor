namespace Fluxor.Blazor.Web.Middlewares.Routing;

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
	/// If true, replaces the current entry in the history stack. If false, appends the new entry to the history stack
	/// </summary>
	public bool Reload { get; }

	/// <summary>
	/// Creates a new instance of the action
	/// </summary>
	/// <param name="newUri">The new address to navigate to</param>
	public GoAction(string newUri) : this(newUri, false)
	{
	}

	/// <summary>
	/// Creates a new instance of the action
	/// </summary>
	/// <param name="newUri">The new address to navigate to</param>
	/// <param name="forceLoad">When true forces a real browser navigation and page reload</param>
	public GoAction(string newUri, bool forceLoad = false) : this(newUri, forceLoad, false)
	{
	}

	/// <summary>
	/// Creates a new instance of the action
	/// </summary>
	/// <param name="newUri">The new address to navigate to</param>
	/// <param name="forceLoad">When true forces a real browser navigation and page reload</param>
	/// <param name="reload">If true, replaces the current entry in the history stack. If false, appends the new entry to the history stack</param>
	public GoAction(string newUri, bool forceLoad = false, bool reload = false)
	{
		NewUri = newUri;
		ForceLoad = forceLoad;
		Reload = reload;
	}
}
