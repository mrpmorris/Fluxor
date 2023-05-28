using Fluxor.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Fluxor
{
	/// <summary>
	/// A class that implements <see cref="IDispatcher"/>
	/// </summary>
	public class Dispatcher : IDispatcher
	{
		private readonly object SyncRoot = new();
		private readonly ConcurrentQueue<object> QueuedActions = new ConcurrentQueue<object>();
		private EventHandler<ActionDispatchedEventArgs> _ActionDispatched;

		/// <see cref="IDispatcher.ActionDispatched"/>
		public event EventHandler<ActionDispatchedEventArgs> ActionDispatched
		{
			add
			{
				lock (SyncRoot)
				{
					_ActionDispatched += value;
				}
				if (QueuedActions.Count > 0)
					DequeueActions();
			}
			remove
			{
				lock (SyncRoot)
				{
					_ActionDispatched -= value;
				}
			}
		}

		/// <see cref="IDispatcher.Dispatch(object)"/>
		public void Dispatch(object action)
		{
			if (action is null)
				throw new ArgumentNullException(nameof(action));

			if (_ActionDispatched is not null)
				_ActionDispatched(this, new ActionDispatchedEventArgs(action));
			else
				QueuedActions.Enqueue(action);
		}

		private void DequeueActions()
		{
			while (QueuedActions.TryDequeue(out object queuedAction))
				_ActionDispatched(this, new ActionDispatchedEventArgs(queuedAction));
		}
	}
}
