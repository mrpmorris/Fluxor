using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fluxor;

/// <summary>
/// A class that implements <see cref="IDispatcher"/>
/// </summary>
public class Dispatcher : IDispatcher
{
	private readonly object SyncRoot = new();
	private readonly Queue<ActionDispatchedEventArgs> QueuedActions = new Queue<ActionDispatchedEventArgs>();
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

	/// <see cref="IDispatcher.DispatchAsync(object)"/>
	public Task DispatchAsync(object action)
	{
		if (action is null)
			throw new ArgumentNullException(nameof(action));

		var dispatchedEvent = new ActionDispatchedEventArgs(action);
		lock (SyncRoot)
		{
			QueuedActions.Enqueue(dispatchedEvent);
		}
		DequeueActions();
		return dispatchedEvent.Completion;
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
			ActionDispatchedEventArgs dequeuedEvent = null;
			EventHandler<ActionDispatchedEventArgs> callbacks;
			lock (SyncRoot)
			{
				callbacks = _ActionDispatched;
				IsDequeuing = callbacks is not null && QueuedActions.TryDequeue(out dequeuedEvent);
				if (!IsDequeuing)
					return;
			}

			callbacks(this, dequeuedEvent);
		} while (true);
	}
}
