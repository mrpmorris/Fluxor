using System.Runtime.CompilerServices;

namespace Fluxor.StoreBuilderSourceGenerator
{
	internal static class StringExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string Unquote(this string source) =>
			source == null
			? null
			: source.StartsWith("\"") && source.EndsWith("\"") && source.Length > 1
			? source.Substring(1, source.Length - 2)
			: source;
	}
}
