using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fluxor;

/// <see cref="IStore"/>
public class Store : IStore, IActionSubscriber, IDisposable
{
	/// <see cref="IStore.Features"/>
	public IReadOnlyDictionary<string, IFeature> Features => FeaturesByName;
	/// <see cref="IStore.Initialized"/>
	public Task Initialized => InitializedCompletionSource.Task;

	private object SyncRoot = new object();
	private bool Disposed;
	private readonly IDispatcher Dispatcher;
	private readonly Dictionary<string, IFeature> FeaturesByName = new(StringComparer.InvariantCultureIgnoreCase);
	private readonly List<IEffect> Effects = new();
	private readonly List<IMiddleware> Middlewares = new();
	private readonly List<IMiddleware> ReversedMiddlewares = new();
	private readonly ConcurrentQueue<ActionDispatchedEventArgs> QueuedActions = new();
	private readonly TaskCompletionSource<bool> InitializedCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
	private readonly ActionSubscriber ActionSubscriber;
	private readonly Task StoreInitializedDispatchTask;

	private volatile bool IsDispatching;
	private volatile int BeginMiddlewareChangeCount;
	private volatile bool HasActivatedStore;
	private bool IsInsideMiddlewareChange => BeginMiddlewareChangeCount > 0;

	/// <summary>
	/// Creates an instance of the store
	/// </summary>
	public Store(IDispatcher dispatcher)
	{
		ActionSubscriber = new ActionSubscriber();
		Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
		Dispatcher.ActionDispatched += ActionDispatched;
		// A constructor cannot await, so hold the task and surface it during activation.
		StoreInitializedDispatchTask = Dispatcher.DispatchAsync(new StoreInitializedAction());
	}

	/// <see cref="IStore.GetMiddlewares"/>
	public IEnumerable<IMiddleware> GetMiddlewares() => Middlewares;

	/// <see cref="IStore.AddFeature(IFeature)"/>
	public void AddFeature(IFeature feature)
	{
		if (feature is null)
			throw new ArgumentNullException(nameof(feature));

		lock (SyncRoot)
		{
			FeaturesByName.Add(feature.GetName(), feature);
		}
	}

	/// <see cref="IStore.AddEffect(IEffect)"/>
	public void AddEffect(IEffect effect)
	{
		if (effect is null)
			throw new ArgumentNullException(nameof(effect));

		lock (SyncRoot)
		{
			Effects.Add(effect);
		}
	}

	/// <see cref="IStore.AddMiddleware(IMiddleware)"/>
	public void AddMiddleware(IMiddleware middleware)
	{
		lock (SyncRoot)
		{
			Middlewares.Add(middleware);
			ReversedMiddlewares.Insert(0, middleware);
			// Initialize the middleware immediately if the store has already been initialized, otherwise this will be
			// done the first time Dispatch is called
			if (HasActivatedStore)
			{
				middleware
					.InitializeAsync(Dispatcher, this)
					.ContinueWith(t =>
					{
						if (!t.IsFaulted)
							middleware.AfterInitializeAllMiddlewares();
					});
			}
		}
	}

	/// <see cref="IStore.BeginInternalMiddlewareChange"/>
	public IDisposable BeginInternalMiddlewareChange()
	{
		IDisposable[] disposables = null;
		lock (SyncRoot)
		{
			BeginMiddlewareChangeCount++;
			disposables = Middlewares
				.Select(x => x.BeginInternalMiddlewareChange())
				.ToArray();
		}

		return new DisposableCallback(
			id: $"{nameof(Store)}.{nameof(BeginInternalMiddlewareChange)}",
			() => EndMiddlewareChange(disposables));
	}

	/// <see cref="IStore.InitializeAsync"/>
	public async Task InitializeAsync()
	{
		if (HasActivatedStore)
			return;
		await ActivateStoreAsync();
	}

	/// <see cref="IActionSubscriber.SubscribeToAction{TAction}(object, Action{TAction})"/>
	public void SubscribeToAction<TAction>(object subscriber, Action<TAction> callback)
	{
		ActionSubscriber.SubscribeToAction(subscriber, callback);
	}

	/// <see cref="IActionSubscriber.UnsubscribeFromAllActions(object)"/>
	public void UnsubscribeFromAllActions(object subscriber)
	{
		ActionSubscriber.UnsubscribeFromAllActions(subscriber);
	}

	/// <see cref="IActionSubscriber.GetActionUnsubscriberAsIDisposable(object)"/>
	public IDisposable GetActionUnsubscriberAsIDisposable(object subscriber) =>
		ActionSubscriber.GetActionUnsubscriberAsIDisposable(subscriber);

	void IDisposable.Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			Dispatcher.ActionDispatched -= ActionDispatched;
		}
	}


	private void ActionDispatched(object sender, ActionDispatchedEventArgs e)
	{
		// Do not allow task dispatching inside a middleware-change.
		// These change cycles are for things like "jump to state" in Redux Dev Tools
		// and should be short lived.
		// We avoid dispatching inside a middleware change because we don't want UI events (like component Init)
		// that trigger actions (such as fetching data from a server) to execute
		if (IsInsideMiddlewareChange)
		{
			// Ignored by design; there is no work to await, so the action's task
			// completes successfully and immediately.
			e.Complete();
			return;
		}

		// This is a concurrent queue, so is safe even if dequeuing is already in progress
		QueuedActions.Enqueue(e);

		// HasActivatedStore is set to true when the page finishes loading
		// At which point DequeueActions will be called
		// So if it hasn't been activated yet, just exit and wait for that to happen
		if (!HasActivatedStore)
			return;

		// If a dequeue is still going then it will deal with the event we just
		// queued, so we can exit at this point.
		// This prevents a re-entrant deadlock
		// An action enqueued between the running loop's final failed TryDequeue and it
		// resetting IsDispatching would otherwise be stranded with a forever-pending task,
		// so retry until the queue is empty or another dequeue is in progress.
		while (!IsDispatching && !QueuedActions.IsEmpty)
		{
			lock (SyncRoot)
			{
				DequeueActions();
			};
		}
	}

	private void EndMiddlewareChange(IDisposable[] disposables)
	{
		lock (SyncRoot)
		{
			BeginMiddlewareChangeCount--;
			if (BeginMiddlewareChangeCount == 0)
				disposables.ToList().ForEach(x => x.Dispose());
		}
	}

	private void TriggerEffects(ActionDispatchedEventArgs dispatchedEvent)
	{
		object action = dispatchedEvent.Action;
		var effectsToExecute = Effects
			.Where(x => x.ShouldReactToAction(action))
			.ToArray();

		if (effectsToExecute.Length == 0)
		{
			// No effects, so the action's work is already complete
			dispatchedEvent.Complete();
			return;
		}

		var recordedExceptions = new List<Exception>();
		var executedEffects = new List<Task>();

		// Execute all tasks. Some will execute synchronously and complete immediately,
		// so we need to catch their exceptions in the loop so they don't prevent
		// other effects from executing.
		foreach (IEffect effect in effectsToExecute)
		{
			try
			{
				// Guard against effects that return a null task, which would otherwise
				// cause Task.WhenAll to throw synchronously
				executedEffects.Add(effect.HandleAsync(action, Dispatcher) ?? Task.CompletedTask);
			}
			catch (Exception e)
			{
				recordedExceptions.Add(e);
			}
		}

		// Complete the dispatched action's task once all of its effects have completed.
		// This must not be awaited by the dequeue loop, otherwise an effect awaiting a
		// nested DispatchAsync would deadlock.
		Task.WhenAll(executedEffects).ContinueWith(t =>
		{
			if (t.IsFaulted)
				recordedExceptions.AddRange(t.Exception.Flatten().InnerExceptions);

			if (recordedExceptions.Count == 0)
				dispatchedEvent.Complete();
			else
				dispatchedEvent.Fail(recordedExceptions);
		}, TaskScheduler.Default);
	}

	private async Task InitializeMiddlewaresAsync()
	{
		foreach (IMiddleware middleware in Middlewares)
		{
			await middleware.InitializeAsync(Dispatcher, this);
		}
		Middlewares.ForEach(x => x.AfterInitializeAllMiddlewares());
	}

	private void ExecuteMiddlewareBeforeDispatch(object actionAboutToBeDispatched)
	{
		foreach (IMiddleware middleWare in Middlewares)
			middleWare.BeforeDispatch(actionAboutToBeDispatched);
	}

	private void ExecuteMiddlewareAfterDispatch(object actionJustDispatched)
	{
		Middlewares.ForEach(x => x.AfterDispatch(actionJustDispatched));
	}

	private async Task ActivateStoreAsync()
	{
		if (HasActivatedStore)
			return;

		await InitializeMiddlewaresAsync();

		lock (SyncRoot)
		{
			HasActivatedStore = true;
			DequeueActions();
			InitializedCompletionSource.TrySetResult(true);
		}

		// Surface any StoreInitializedAction reducer/effect failures to InitializeAsync callers
		await StoreInitializedDispatchTask;
	}

	private void DequeueActions()
	{
		if (IsDispatching)
			return;

		IsDispatching = true;
		try
		{
			while (QueuedActions.TryDequeue(out ActionDispatchedEventArgs dispatchedEvent))
			{
				object nextActionToProcess = dispatchedEvent.Action;

				// Only process the action if no middleware vetos it.
				// A veto is normal control flow, not an error, so the task completes successfully.
				if (!Middlewares.All(x => x.MayDispatchAction(nextActionToProcess)))
				{
					dispatchedEvent.Complete();
					continue;
				}

				try
				{
					ExecuteMiddlewareBeforeDispatch(nextActionToProcess);

					// Notify all features of this action
					foreach (var featureInstance in FeaturesByName.Values)
						featureInstance.ReceiveDispatchNotificationFromStore(nextActionToProcess);

					ActionSubscriber?.Notify(nextActionToProcess);
					ExecuteMiddlewareAfterDispatch(nextActionToProcess);
				}
				catch (Exception e)
				{
					// Fault only this action's task so one bad action cannot wedge the queue;
					// its effects are not triggered.
					dispatchedEvent.Fail(e);
					continue;
				}

				TriggerEffects(dispatchedEvent);
			}
		}
		finally
		{
			IsDispatching = false;
		}
	}
}
