using Fluxor.Extensions;
using Fluxor.UnsupportedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Fluxor
{
	/// <see cref="IFeature{TState}"/>
	public abstract class Feature<TState> : IFeature<TState>
	{
		/// <see cref="IFeature.MaximumStateChangedNotificationsPerSecond"/>
		public byte MaximumStateChangedNotificationsPerSecond { get; set; }

		/// <see cref="IFeature.GetName"/>
		public abstract string GetName();

		/// <see cref="IFeature.GetState"/>
		public virtual object GetState() => State;

		/// <see cref="IFeature.RestoreState(object)"/>
		public virtual void RestoreState(object value) => State = (TState)value;

		/// <see cref="IFeature.GetStateType"/>
		public virtual Type GetStateType() => typeof(TState);

		/// <summary>
		/// Gets the initial state for the feature
		/// </summary>
		/// <returns>The initial state</returns>
		protected abstract TState GetInitialState();

		/// <summary>
		/// A list of reducers registered with this feature
		/// </summary>
		protected readonly List<IReducer<TState>> Reducers = new List<IReducer<TState>>();

		private bool HasInitialState;
		private SpinLock SpinLock = new SpinLock();
		private readonly ThrottledInvoker TriggerStateChangedCallbacksThrottler;

		/// <summary>
		/// Creates a new instance
		/// </summary>
		public Feature()
		{
			TriggerStateChangedCallbacksThrottler = new ThrottledInvoker(TriggerStateChangedCallbacks);
		}

		private EventHandler untypedStateChanged;
		event EventHandler IFeature.StateChanged
		{
			add
			{
				SpinLock.ExecuteLocked(() => untypedStateChanged += value );
			}

			remove
			{
				SpinLock.ExecuteLocked(() => untypedStateChanged -= value);
			}
		}

		private EventHandler<TState> stateChanged;
		/// <summary>
		/// Event that is executed whenever the state changes
		/// </summary>
		public event EventHandler<TState> StateChanged
		{
			add
			{
				SpinLock.ExecuteLocked(() => stateChanged += value);
			}

			remove
			{
				SpinLock.ExecuteLocked(() => stateChanged -= value);
			}
		}

		private TState _State;
		/// <see cref="IFeature{TState}.State"/>
		public virtual TState State
		{
			get
			{
				if (HasInitialState)
					return _State;
				SpinLock.ExecuteLocked(() =>
				{
					if (!HasInitialState)
					{
						_State = (TState)GetInitialState();
						HasInitialState = true;
					}
				});
				return _State;
			}
			protected set =>
				SpinLock.ExecuteLocked(() =>
				{
					bool stateHasChanged = !Object.ReferenceEquals(_State, value);
					if (stateHasChanged)
					{
						_State = value;
						TriggerStateChangedCallbacksThrottler.Invoke(MaximumStateChangedNotificationsPerSecond);
					}
				});
		}

		/// <see cref="IFeature{TState}.AddReducer(IReducer{TState})"/>
		public virtual void AddReducer(IReducer<TState> reducer)
		{
			if (reducer == null)
				throw new ArgumentNullException(nameof(reducer));
			Reducers.Add(reducer);
		}

		/// <see cref="IFeature.ReceiveDispatchNotificationFromStore(object)"/>
		public virtual void ReceiveDispatchNotificationFromStore(object action)
		{
			if (action == null)
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
			stateChanged?.Invoke(this, State);
			untypedStateChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}
