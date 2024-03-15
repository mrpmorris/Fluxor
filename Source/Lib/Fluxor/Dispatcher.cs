using System;
using System.Collections.Generic;

namespace Fluxor;

/// <summary>
/// A class that implements <see cref="IDispatcher"/>
/// </summary>
public class Dispatcher : IDispatcher
{
	private readonly object SyncRoot = new();
	private readonly Queue<object> QueuedActions = new Queue<object>();
	private volatile bool IsDequeuing;
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

		lock (SyncRoot)
		{
			QueuedActions.Enqueue(action);
		}
		DequeueActions();
	}

	private void DequeueActions()
	{
		lock (SyncRoot)
		{
			if (IsDequeuing || _ActionDispatched is null)
				return;
			IsDequeuing = true;
		}
		do
		{
			object dequeuedAction = null;
			EventHandler<ActionDispatchedEventArgs> callbacks;
			lock (SyncRoot)
			{
				callbacks = _ActionDispatched;
				IsDequeuing = callbacks is not null && QueuedActions.TryDequeue(out dequeuedAction);
				if (!IsDequeuing)
					return;
			}

			callbacks(this, new ActionDispatchedEventArgs(dequeuedAction));
		} while (true);
	}
}
