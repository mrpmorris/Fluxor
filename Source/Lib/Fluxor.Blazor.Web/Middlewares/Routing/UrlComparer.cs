using System;

namespace Fluxor.Blazor.Web.Middlewares.Routing
{
	internal static class UrlComparer
	{
		public static bool SameAs(this Uri first, Uri second,
			UriComponents components = UriComponents.HostAndPort | UriComponents.Path, 
			UriFormat format = UriFormat.SafeUnescaped, 
			StringComparison comparisonType = StringComparison.OrdinalIgnoreCase) =>
			Uri.Compare(first, second, components, format, comparisonType) == 0;
	}
}
