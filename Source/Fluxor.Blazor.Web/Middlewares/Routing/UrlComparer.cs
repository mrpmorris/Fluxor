using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fluxor.Blazor.Web.Middlewares.Routing
{
	internal static class UrlComparer
	{
		public static bool AreEqual(string first, string second) =>
			string.Equals(first?.TrimEnd('/'), second?.TrimEnd('/'), StringComparison.OrdinalIgnoreCase);
	}
}
