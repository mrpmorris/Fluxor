using Fluxor.Extensions;
using System;
using System.Threading;

namespace Fluxor
{
	/// <summary>
	/// A class that implements <see cref="IDispatcher"/>
	/// </summary>
	public class Dispatcher : IDispatcher
	{
		private SpinLock SpinLock = new();
		private EventHandler<ActionDispatchedEventArgs> _ActionDispatched;

		/// <see cref="IDispatcher.ActionDispatched"/>
		public event EventHandler<ActionDispatchedEventArgs> ActionDispatched
		{
			add
			{
				SpinLock.ExecuteLocked(() => _ActionDispatched += value);
			}
			remove
			{
				SpinLock.ExecuteLocked(() => _ActionDispatched -= value);
			}
		}

		/// <see cref="IDispatcher.Dispatch(object)"/>
		public void Dispatch(object action)
		{
			if (action is null)
				throw new ArgumentNullException(nameof(action));

			_ActionDispatched?.Invoke(this, new ActionDispatchedEventArgs(action));
		}
	}
}
