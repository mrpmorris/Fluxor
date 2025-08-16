using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fluxor.Extensions;

internal static class SemaphoreSlimExtensions
{
	public static async ValueTask ExecuteLockedAsync(this SemaphoreSlim instance, Func<ValueTask> func)
	{
		await instance.WaitAsync();
		try
		{
			await func();
		}
		finally
		{
			instance.Release();
		}
	}
}
