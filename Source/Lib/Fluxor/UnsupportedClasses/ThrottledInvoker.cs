using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fluxor.UnsupportedClasses;

public sealed class ThrottledInvoker : IDisposable
{
	private readonly object SyncRoot = new();
	private readonly Action ThrottledAction;
	private DateTime NextAllowedInvokeUtc;
	private bool HasPendingImmediateInvocation;
	private bool HasPendingDeferredInvocation;
	private bool IsDisposed;
	private CancellationTokenSource CancellationTokenSource;

	public ThrottledInvoker(Action throttledAction)
	{
		ThrottledAction = throttledAction ?? throw new ArgumentNullException(nameof(throttledAction));
		NextAllowedInvokeUtc = DateTime.UtcNow;
		CancellationTokenSource = new CancellationTokenSource();
	}

	public void Invoke(byte maximumInvokesPerSecond)
	{
		if (IsDisposed)
			return;

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
			else if (shouldExecuteDeferred)
				HasPendingDeferredInvocation = true;
		}

		if (shouldExecuteImmediately)
			Invoke(throttleWindowMS, wasImmediateInvoke: true);
		else
			_ = InvokeDeferredAsync(throttleWindowMS);
	}

	void IDisposable.Dispose()
	{
		if (IsDisposed) return;
		CancellationTokenSource.Cancel();
		IsDisposed = true;
	}

	private void Invoke(int throttleWindowMS, bool wasImmediateInvoke)
	{
		try
		{
			if (!CancellationTokenSource.IsCancellationRequested)
				ThrottledAction();
		}
		finally
		{
			lock (SyncRoot)
			{
				NextAllowedInvokeUtc = DateTime.UtcNow.AddMilliseconds(throttleWindowMS);

				if (wasImmediateInvoke)
					HasPendingImmediateInvocation = false;
				else
					HasPendingDeferredInvocation = false;
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

			await Task.Delay(totalMillisecondsToWait, CancellationTokenSource.Token);
		}
		while (!CancellationTokenSource.IsCancellationRequested);
	}
}
