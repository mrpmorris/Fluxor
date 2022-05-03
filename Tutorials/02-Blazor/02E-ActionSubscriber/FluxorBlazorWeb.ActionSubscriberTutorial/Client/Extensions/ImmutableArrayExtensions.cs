using System.Collections.Immutable;

namespace FluxorBlazorWeb.ActionSubscriberTutorial.Client.Extensions;

public static class ImmutableArrayExtensions
{
	public static bool ReplaceOne<T>(this ImmutableArray<T> source, Predicate<T> selector, Func<T, T> replacement, out ImmutableArray<T> result)
	{
		for(int o = 0; o < source.Length; o++)
		{
			T item = source[o];
			if (selector(item))
			{
				result = source.SetItem(o, replacement(item));
				return true;
			}
		}
		
		result = source;
		return false;
	}
}
