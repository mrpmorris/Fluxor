using System;

namespace Fluxor.PreScanningStoreBuilder.Helpers
{
	internal static class HashCode
	{
		public static int Combine(params object[] objects)
		{
			unchecked
			{
				int hash = 17;

				foreach (object obj in objects ?? Array.Empty<object>())
					hash = hash * 23 + (obj?.GetHashCode() ?? 0);

				return hash;
			}
		}
	}
}