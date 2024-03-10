using System;
using System.Threading.Tasks;

namespace Fluxor.UnsupportedClasses;

public class ThrottledInvoker
{
	private readonly object SyncRoot = new();
	private readonly Action ThrottledAction;
	private DateTime NextAllowedInvokeUtc;
	private bool HasPendingImmediateInvocation;
	private bool HasPendingDeferredInvocation;

	public ThrottledInvoker(Action throttledAction)
	{
		ThrottledAction = throttledAction ?? throw new ArgumentNullException(nameof(throttledAction));
		NextAllowedInvokeUtc = DateTime.UtcNow;
	}

	public void Invoke(byte maximumInvokesPerSecond)
	{
		int throttleWindowMS =
			maximumInvokesPerSecond == 0
			? 0
			: 1000 / maximumInvokesPerSecond;

		if (throttleWindowMS == 0)
		{
			ThrottledAction();
			return;
		}

		bool shouldExecuteImmediately = false;
		lock (SyncRoot)
		{
			if (HasPendingDeferredInvocation)
				return;

			DateTime nowUtc = DateTime.UtcNow;
			shouldExecuteImmediately =
				!HasPendingImmediateInvocation
				&& nowUtc >= NextAllowedInvokeUtc;

			bool shouldExecuteDeferred =
				!shouldExecuteImmediately
				&& !HasPendingDeferredInvocation;

			if (shouldExecuteImmediately)
				HasPendingImmediateInvocation = true;

			if (shouldExecuteDeferred)
				HasPendingDeferredInvocation = true;
		}

		if (shouldExecuteImmediately)
			Invoke(throttleWindowMS, wasImmediateInvoke: true);
		else
			_ = InvokeDeferredAsync(throttleWindowMS);
	}

	private void Invoke(int throttleWindowMS, bool wasImmediateInvoke)
	{
		try
		{
			ThrottledAction();
		}
		finally
		{
			lock (SyncRoot)
			{
				if (wasImmediateInvoke)
					HasPendingImmediateInvocation = false;
				else
					HasPendingDeferredInvocation = false;

				NextAllowedInvokeUtc = DateTime.UtcNow.AddMilliseconds(throttleWindowMS);
			}
		}
	}

	private async ValueTask InvokeDeferredAsync(int throttleWindowMS)
	{
		await WaitUntilAfterAsync(DateTime.UtcNow.AddMilliseconds(throttleWindowMS));
		Invoke(throttleWindowMS: throttleWindowMS, wasImmediateInvoke: false);
	}

	private async ValueTask WaitUntilAfterAsync(DateTime targetTimeUtc)
	{
		await Task.Yield();
		do
		{
			int totalMillisecondsToWait = (int)Math.Ceiling((targetTimeUtc - DateTime.UtcNow).TotalMilliseconds);
			if (totalMillisecondsToWait <= 0)
				break;

			if (totalMillisecondsToWait > 1000)
				totalMillisecondsToWait = 1000;

			await Task.Delay(totalMillisecondsToWait);
		}
		while (true);
	}
}
