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
		/// Checks if the given <see cref="System.Uri">address</see> matches the <see cref="Uri">current one</see>.
		/// </summary>
		/// <param name="address">Address to match with the current browser address.</param>
		/// <param name="components">A bitwise combination of the <see cref="T:System.UriComponents"></see> values that specifies which parts of the addresses to compare.</param>
		/// <param name="format">One of the <see cref="T:System.UriFormat"></see> values that controls how special characters are escaped.</param>
		/// <param name="comparisonType">One of the enumeration values <see cref="T:System.StringComparison"></see> that determines how address parts are compared.</param>
		/// <returns><see cref="T:System.Boolean">true</see> if the <see cref="System.Uri">address</see> is the same as current <see cref="Uri">browser address</see>; otherwise, <see cref="T:System.Boolean">false</see>.</returns>
		public bool IsCurrent(Uri address,
			UriComponents components = UriComponents.HostAndPort | UriComponents.Path,
			UriFormat format = UriFormat.SafeUnescaped,
			StringComparison comparisonType = StringComparison.OrdinalIgnoreCase) =>
			GetAsUri().SameAs(address, components, format, comparisonType);

		/// <summary>
		/// Creates a new instance of the state
		/// </summary>
		/// <param name="uri">The new value of the URI state</param>
#if NET5_0_OR_GREATER
		[System.Text.Json.Serialization.JsonConstructor]
#endif
		public RoutingState(string uri)
		{
			Uri = uri;
		}
	}
}
