using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

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
	/// <remarks>
	/// Adding a subscriber delivers any actions already queued, so it can throw in the same way as
	/// <see cref="Dispatch(object)"/>.
	/// </remarks>
	/// <exception cref="AggregateException">
	/// Thrown when more than one <see cref="ActionDispatched"/> subscriber call throws while the queue is
	/// drained; it holds the exceptions in the order they were thrown. A single exception is rethrown
	/// unchanged (same instance, type and stack trace). Either way it is thrown once the drain has ended:
	/// every queued action has been delivered, unless no subscriber remains (for example one unsubscribed
	/// while throwing), in which case the remaining actions stay queued until a subscriber is added.
	/// </exception>
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
	/// <exception cref="AggregateException">
	/// Thrown when more than one <see cref="ActionDispatched"/> subscriber call throws while the queue is
	/// drained; it holds the exceptions in the order they were thrown. A single exception is rethrown
	/// unchanged (same instance, type and stack trace). Either way it is thrown once the drain has ended:
	/// every queued action has been delivered, unless no subscriber remains (for example one unsubscribed
	/// while throwing), in which case the remaining actions stay queued until a subscriber is added.
	/// </exception>
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
		List<Exception> exceptions = null;
		try
		{
			do
			{
				object dequeuedAction = null;
				EventHandler<ActionDispatchedEventArgs> callbacks;
				lock (SyncRoot)
				{
					callbacks = _ActionDispatched;
					IsDequeuing = callbacks is not null && QueuedActions.TryDequeue(out dequeuedAction);
					if (!IsDequeuing)
						break;
				}

				try
				{
					callbacks(this, new ActionDispatchedEventArgs(dequeuedAction));
				}
				catch (Exception ex)
				{
					// Keep draining so actions queued behind this one are not stranded
					(exceptions ??= new List<Exception>()).Add(ex);
				}
			} while (true);
		}
		catch
		{
			// The loop clears IsDequeuing inside the same lock that observes the queue is empty, so the
			// normal exit path must be left alone. Only an abnormal exit needs to release it here, and
			// doing so is safe because no other thread can have started dequeuing while the flag was set.
			lock (SyncRoot)
			{
				IsDequeuing = false;
			}
			throw;
		}

		// The drain ended (queue empty or no subscribers) and IsDequeuing was cleared inside the lock above;
		// now pass the collected failures on
		if (exceptions is null)
			return;
		if (exceptions.Count == 1)
			ExceptionDispatchInfo.Capture(exceptions[0]).Throw();
		throw new AggregateException(exceptions);
	}
}
