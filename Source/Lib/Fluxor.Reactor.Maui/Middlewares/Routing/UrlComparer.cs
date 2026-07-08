using System;

namespace Fluxor.Reactor.Maui.Middlewares.Routing;

internal static class UrlComparer
{
	public static bool AreEqual(string first, string second) =>
		Uri.Compare(
			uri1: new Uri(first),
			uri2: new Uri(second),
			partsToCompare: UriComponents.HostAndPort | UriComponents.PathAndQuery,
			compareFormat: UriFormat.SafeUnescaped,
			comparisonType: StringComparison.OrdinalIgnoreCase) == 0;
}
