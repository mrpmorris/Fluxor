using Fluxor.UnsupportedClasses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluxor;

/// <inheritdoc/>
public abstract class Feature<TState> : IFeature<TState>
{
	/// <inheritdoc/>
	public byte MaximumStateChangedNotificationsPerSecond { get; set; }

	/// <inheritdoc/>
	public abstract string GetName();

	/// <inheritdoc/>
	public virtual object GetState() => State;

	/// <inheritdoc/>
	public virtual bool DebuggerBrowsable { get; set; } = true;

	/// <inheritdoc/>
	public virtual void RestoreState(object value) => State = (TState)value;

	/// <inheritdoc/>
	public virtual Type GetStateType() => typeof(TState);

	/// <summary>
	/// Gets the initial state for the feature
	/// </summary>
	/// <returns>The initial state</returns>
	protected abstract TState GetInitialState();

	/// <summary>
	/// A list of reducers registered with this feature
	/// </summary>
	protected readonly List<IReducer<TState>> Reducers = new();

	private readonly object SyncRoot = new();
	private bool HasInitialState;
	private readonly ThrottledInvoker TriggerStateChangedCallbacksThrottler;

	/// <summary>
	/// Creates a new instance
	/// </summary>
	public Feature()
	{
		TriggerStateChangedCallbacksThrottler = new(TriggerStateChangedCallbacks);
	}

	private EventHandler _StateChanged;
	public event EventHandler StateChanged
	{
		add
		{
			lock (SyncRoot)
			{
				_StateChanged += value;
			}
		}

		remove
		{
			lock (SyncRoot)
			{
				_StateChanged -= value;
			}
		}
	}

	private TState _State;
	/// <inheritdoc/>
	public virtual TState State
	{
		get
		{
			if (HasInitialState)
				return _State;
			if (!HasInitialState)
			{
				lock (SyncRoot)
				{
					if (!HasInitialState)
					{
						_State = (TState)GetInitialState();
						HasInitialState = true;
					}
				}
			}
			return _State;
		}
		protected set
		{
			bool stateHasChanged = !Object.ReferenceEquals(_State, value);
			if (stateHasChanged)
			{
				lock (SyncRoot)
				{
					_State = value;
					HasInitialState = true;
				}
				TriggerStateChangedCallbacksThrottler.Invoke(MaximumStateChangedNotificationsPerSecond);
			}
		}
	}

	/// <inheritdoc/>
	public virtual void AddReducer(IReducer<TState> reducer)
	{
		if (reducer is null)
			throw new ArgumentNullException(nameof(reducer));
		Reducers.Add(reducer);
	}

	/// <inheritdoc/>
	public virtual void ReceiveDispatchNotificationFromStore(object action)
	{
		if (action is null)
			throw new ArgumentNullException(nameof(action));

		IEnumerable<IReducer<TState>> applicableReducers = Reducers.Where(x => x.ShouldReduceStateForAction(action));
		TState newState = State;
		foreach (IReducer<TState> currentReducer in applicableReducers)
		{
			newState = currentReducer.Reduce(newState, action);
		}
		State = newState;
	}

	private void TriggerStateChangedCallbacks()
	{
		_StateChanged?.Invoke(this, EventArgs.Empty);
	}
}
