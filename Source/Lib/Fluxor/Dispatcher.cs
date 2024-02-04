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
		private readonly object SyncRoot = new();
		private readonly Queue<object> QueuedActions = new Queue<object>();
		private EventHandler<ActionDispatchedEventArgs> _ActionDispatched;

		/// <see cref="IDispatcher.ActionDispatched"/>
		public event EventHandler<ActionDispatchedEventArgs> ActionDispatched
		{
			add
			{
				lock (SyncRoot)
				{
					_ActionDispatched += value;
					if (QueuedActions.Count > 0)
						DequeueActions();
				}
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

			bool actionDispatchedIsNull;
			lock (SyncRoot)
			{
				actionDispatchedIsNull = _ActionDispatched is null;
                if (actionDispatchedIsNull)
					QueuedActions.Enqueue(action);
			}

			if (!actionDispatchedIsNull)
				_ActionDispatched(this, new ActionDispatchedEventArgs(action));
        }

        private void DequeueActions()
		{
			foreach (object queuedAction in QueuedActions)
				_ActionDispatched(this, new ActionDispatchedEventArgs(queuedAction));
		}
	}
}
