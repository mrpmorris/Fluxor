using System;

namespace Fluxor.Blazor.Web.Middlewares.Routing
{
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
		/// If true, bypasses client-side routing and forces the browser to load the new
		//  page from the server, whether or not the URI would normally be handled by the
		//  client-side router.
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
}
