using System;
using System.Threading;

namespace Fluxor.UnsupportedClasses
{
	public class ThrottledInvoker
	{
		public ushort ThrottleWindowMs { get; set; }

		private volatile int LockFlag;
		private volatile bool InvokingSuspended;
		private DateTime LastInvokeTime;
		private readonly Action ThrottledAction;
		private Timer ThrottleTimer;

		public ThrottledInvoker(Action throttledAction)
		{
			ThrottledAction = throttledAction ?? throw new ArgumentNullException(nameof(throttledAction));
			LastInvokeTime = DateTime.UtcNow - TimeSpan.FromMilliseconds(ushort.MaxValue);
		}

		public void Invoke()
		{
			Invoke(reentering: false);
		}

		public void Invoke(byte maximumInvokesPerSecond)
		{
			if (maximumInvokesPerSecond == 0)
				ThrottleWindowMs = 0;
			else
				ThrottleWindowMs = (ushort)(1000 / maximumInvokesPerSecond);

			Invoke(reentering: false);
		}

		private void Invoke(bool reentering)
		{
			// If no throttle window then bypass throttling
			if (ThrottleWindowMs == 0)
			{
				ThrottledAction();
				return;
			}

			LockAndExecuteOnlyIfNotAlreadyLocked(
				reentering: reentering,
				lockedAction: () =>
				{
					// If waiting for a previously throttled notification to execute
					// then ignore this notification request
					if (InvokingSuspended && !reentering)
						return;

					// Either 1st entry or re-entry. Now we are locked
					// we should dispose of the timer if not null.
					ThrottleTimer?.Dispose();
					ThrottleTimer = null;

					double millisecondsSinceLastInvoke =
						Math.Floor((DateTime.UtcNow - LastInvokeTime).TotalMilliseconds);

					// If last execute was outside the throttle window then execute immediately
					if (millisecondsSinceLastInvoke >= ThrottleWindowMs)
					{
						ExecuteThrottledAction();
						return;
					}

					// This is exactly the second invoke within the time window,
					// so set a timer that will trigger at the start of the next
					// time window and prevent further invokes until
					// the timer has triggered
					InvokingSuspended = true;

					int delayRequired = (int)Math.Ceiling(ThrottleWindowMs - millisecondsSinceLastInvoke);
					ThrottleTimer = new Timer(
						callback: _ => Invoke(reentering: true),
						state: null,
						dueTime: delayRequired,
						period: 0);
				}
			);
		}

		private void LockAndExecuteOnlyIfNotAlreadyLocked(bool reentering, Action lockedAction)
		{
			if (InvokingSuspended && !reentering)
				return;

			bool lockTaken =
				reentering
				|| (Interlocked.CompareExchange(ref LockFlag, 1, 0) == 0);

			if (!lockTaken)
				return;

			try
			{
				lockedAction();
			}
			finally
			{
				LockFlag = 0;
			}
		}

		private void ExecuteThrottledAction()
		{
			try
			{
				ThrottledAction();
			}
			finally
			{
				LastInvokeTime = DateTime.UtcNow;
				// This must be set last, as it is the circuit breaker within the lock code
				InvokingSuspended = false;
			}
		}
	}
}
