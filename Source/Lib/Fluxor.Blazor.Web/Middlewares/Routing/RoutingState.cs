using System;

namespace Fluxor.Blazor.Web.Middlewares.Routing
{
	/// <summary>
	/// State used by <see cref="RoutingMiddleware"/> and <see cref="Feature"/>.
	/// </summary>
	public class RoutingState
	{
		/// <summary>
		/// The browser address
		/// </summary>
		public string Uri { get; }

		/// <summary>
		/// Gets the <see cref="Uri"/> string property as a <see cref="System.Uri"/> object
		/// </summary>
		/// <returns>The <see cref="System.Uri"/> object</returns>
		public Uri GetAsUri() => new Uri(Uri);

		/// <summary>
		/// Creates a new instance of the state
		/// </summary>
		/// <param name="uri">The new value of the URI state</param>
		public RoutingState(string uri)
		{
			Uri = uri;
		}
	}
}
