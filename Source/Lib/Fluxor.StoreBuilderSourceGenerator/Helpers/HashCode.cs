using System.Runtime.CompilerServices;

namespace Fluxor.StoreBuilderSourceGenerator.Helpers;

internal static class HashCode
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Combine(params object[] objects)
	{
		int result = 0;
		unchecked
		{
			for (int i = 0; i < objects.Length; i++)
			{
				object obj = objects[i];
				int objHashCode = obj?.GetHashCode() ?? 0;
				result = result * 397 ^ objHashCode;
			}
		}
		return result;
	}
}
