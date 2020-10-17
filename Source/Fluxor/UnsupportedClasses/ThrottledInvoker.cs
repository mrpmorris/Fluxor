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
		private Action Action;
		private Timer ThrottleTimer;

		public ThrottledInvoker(Action action)
		{
			Action = action ?? throw new ArgumentNullException(nameof(action));
			LastInvokeTime = DateTime.UtcNow - TimeSpan.FromMilliseconds(ushort.MaxValue);
		}

		public void Invoke(byte maximumInvokesPerSecond)
		{
			if (maximumInvokesPerSecond == 0)
				ThrottleWindowMs = 0;
			else
				ThrottleWindowMs = (ushort)(1000 / maximumInvokesPerSecond);
			Invoke();
		}

		public void Invoke()
		{
			// If no throttle window then bypass throttling
			if (ThrottleWindowMs == 0)
			{
				Action();
				return;
			}

			LockAndExecuteOnlyIfNotAlreadyLocked(() =>
			{
				// If waiting for a previously throttled notification to execute
				// then ignore this notification request
				if (InvokingSuspended)
					return;

				int millisecondsSinceLastInvoke =
					(int)(DateTime.UtcNow - LastInvokeTime).TotalMilliseconds;

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
				int delay = ThrottleWindowMs - millisecondsSinceLastInvoke;
				ThrottleTimer = new Timer(
					callback: _ => ExecuteThrottledAction(),
					state: null,
					dueTime: delay,
					period: 0);
			});
		}

		private void LockAndExecuteOnlyIfNotAlreadyLocked(Action action)
		{
			bool lockTaken =
				(Interlocked.CompareExchange(ref LockFlag, 1, 0) == 0);
			if (!lockTaken)
				return;

			try
			{
				action();
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
				Action();
			}
			finally
			{
				ThrottleTimer?.Dispose();
				ThrottleTimer = null;
				LastInvokeTime = DateTime.UtcNow;
				// This must be set last, as it is the circuit breaker within the lock code
				InvokingSuspended = false;
			}
		}
	}
}
