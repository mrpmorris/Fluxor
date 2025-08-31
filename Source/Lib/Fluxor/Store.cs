using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
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
	public bool WasPersisted { get; private set; }

	private object SyncRoot = new object();
	private bool Disposed;
	private readonly IDispatcher Dispatcher;
	private readonly Dictionary<string, IFeature> FeaturesByName = new(StringComparer.InvariantCultureIgnoreCase);
	private readonly List<IEffect> Effects = new();
	private readonly List<IMiddleware> Middlewares = new();
	private readonly List<IMiddleware> ReversedMiddlewares = new();
	private readonly ConcurrentQueue<object> QueuedActions = new();
	private readonly TaskCompletionSource<bool> InitializedCompletionSource = new();
	private readonly ActionSubscriber ActionSubscriber;

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
		Dispatcher.Dispatch(new StoreInitializedAction(this));
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
	public async Task InitializeAsync(IDictionary<string, object> persistedState = null)
	{
		if (HasActivatedStore)
			return;

		WasPersisted = persistedState is not null;
		if (WasPersisted)
		{
			foreach(KeyValuePair<string, IFeature> kvp in Features)
			{
				if (persistedState.TryGetValue(kvp.Key, out object featureState))
					kvp.Value.RestoreState(featureState);
			}
		}

		await ActivateStoreAsync();
	}

	public event EventHandler<Exceptions.UnhandledExceptionEventArgs> UnhandledException;

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

	/// <inheritdoc/>
	public FrozenDictionary<string, object> GetState(bool onlyDebuggerBrowsable)
	{
		var state = new Dictionary<string, object>();
		var serializableFeatures = Features.Values.Where(x => !onlyDebuggerBrowsable || x.DebuggerBrowsable);
		foreach (IFeature feature in serializableFeatures.OrderBy(x => x.GetName()))
			state[feature.GetName()] = feature.GetState();
		return state.ToFrozenDictionary();
	}

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
			return;


		// This is a concurrent queue, so is safe even if dequeuing is already in progress
		QueuedActions.Enqueue(e.Action);

		// HasActivatedStore is set to true when the page finishes loading
		// At which point DequeueActions will be called
		// So if it hasn't been activated yet, just exit and wait for that to happen
		if (!HasActivatedStore)
			return;

		// If a dequeue is still going then it will deal with the event we just
		// queued, so we can exit at this point.
		// This prevents a re-entrant deadlock
		if (!IsDispatching)
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

	private void TriggerEffects(object action)
	{
		var recordedExceptions = new List<Exception>();
		var effectsToExecute = Effects
			.Where(x => x.ShouldReactToAction(action))
			.ToArray();
		var executedEffects = new List<Task>();

		Action<Exception> collectExceptions = e =>
		{
			if (e is AggregateException aggregateException)
				recordedExceptions.AddRange(aggregateException.Flatten().InnerExceptions);
			else
				recordedExceptions.Add(e);
		};

		// Execute all tasks. Some will execute synchronously and complete immediately,
		// so we need to catch their exceptions in the loop so they don't prevent
		// other effects from executing.
		// It's then up to the UI to decide if any of those exceptions should cause
		// the app to terminate or not.
		foreach (IEffect effect in effectsToExecute)
		{
			try
			{
				executedEffects.Add(effect.HandleAsync(action, Dispatcher));
			}
			catch (Exception e)
			{
				collectExceptions(e);
			}
		}

		Task.Run(async () =>
		{
			try
			{
				await Task.WhenAll(executedEffects);
			}
			catch (Exception e)
			{
				collectExceptions(e);
			}

			// Let the UI decide if it wishes to deal with any unhandled exceptions.
			// By default it should throw the exception if it is not handled.
			foreach (Exception exception in recordedExceptions)
				UnhandledException?.Invoke(this, new Exceptions.UnhandledExceptionEventArgs(exception));
		});
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
	}

	private void DequeueActions()
	{
		if (IsDispatching)
			return;

		IsDispatching = true;
		try
		{
			while (QueuedActions.TryDequeue(out object nextActionToProcess))
			{
				// Only process the action if no middleware vetos it
				if (Middlewares.All(x => x.MayDispatchAction(nextActionToProcess)))
				{
					ExecuteMiddlewareBeforeDispatch(nextActionToProcess);

					// Notify all features of this action
					foreach (var featureInstance in FeaturesByName.Values)
						featureInstance.ReceiveDispatchNotificationFromStore(nextActionToProcess);

					ActionSubscriber?.Notify(nextActionToProcess);
					ExecuteMiddlewareAfterDispatch(nextActionToProcess);
					TriggerEffects(nextActionToProcess);
				}
			}
		}
		finally
		{
			IsDispatching = false;
		}
	}
}
