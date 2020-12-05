using Fluxor.Extensions;
using System;
using System.Threading;

namespace Fluxor
{
	public class Dispatcher : IDispatcher
	{
		private EventHandler<ActionDispatchingEventArgs> actionDispatching;
		private SpinLock SpinLock = new SpinLock();

		public event EventHandler<ActionDispatchingEventArgs> ActionDispatching
		{
			add
			{
				SpinLock.ExecuteLocked(() => actionDispatching += value);
			}
			remove
			{
				SpinLock.ExecuteLocked(() => actionDispatching -= value);
			}
		}

		public void Dispatch(object action)
		{
			if (action == null)
				throw new ArgumentNullException(nameof(action));

			actionDispatching?.Invoke(this, new ActionDispatchingEventArgs(action));
		}
	}
}
