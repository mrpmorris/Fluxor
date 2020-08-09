using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fluxor.Extensions
{
	public static class SpinLockExtensions
	{
		public static void ExecuteLocked(this SpinLock spinLock, Action callback)
		{
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));

			bool lockTaken = false;
			try
			{
				do
				{
					spinLock.Enter(ref lockTaken);
				} while (!lockTaken);
				callback();
			}
			finally
			{
				if (lockTaken)
					spinLock.Exit();
			}
		}

		public static async Task ExecuteLockedAsync(this SpinLock spinLock, Func<Task> callback)
		{
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));

			bool lockTaken = false;
			try
			{
				while (!lockTaken)
					spinLock.Enter(ref lockTaken);
				await callback().ConfigureAwait(false);
			}
			finally
			{
				if (lockTaken)
					spinLock.Exit();
			}
		}
	}
}
