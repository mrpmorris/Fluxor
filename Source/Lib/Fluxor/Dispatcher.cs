using Fluxor.Extensions;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Fluxor
{
	/// <summary>
	/// A class that implements <see cref="IDispatcher"/>
	/// </summary>
	public class Dispatcher : IDispatcher
	{
		private SpinLock SpinLock = new();
		private readonly Queue<object> QueuedActions = new Queue<object>();
		private EventHandler<ActionDispatchedEventArgs> _ActionDispatched;

		/// <see cref="IDispatcher.ActionDispatched"/>
		public event EventHandler<ActionDispatchedEventArgs> ActionDispatched
		{
			add
			{
				SpinLock.ExecuteLocked(() =>
				{
					_ActionDispatched += value;
					if (QueuedActions.Count > 0)
						DequeueActions();
				});
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

			SpinLock.ExecuteLocked(() =>
			{
				if (_ActionDispatched is not null)
					_ActionDispatched(this, new ActionDispatchedEventArgs(action));
				else
					QueuedActions.Enqueue(action);
			});
		}

		private void DequeueActions()
		{
			foreach (object queuedAction in QueuedActions)
				_ActionDispatched(this, new ActionDispatchedEventArgs(queuedAction));
		}
	}
}
