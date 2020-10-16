using Fluxor.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Fluxor
{
	/// <see cref="IFeature{TState}"/>
	public abstract class Feature<TState> : IFeature<TState>
	{
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

		protected virtual byte GetMaximumNotificationsPerSecond() => 0;

		/// <summary>
		/// A list of reducers registered with this feature
		/// </summary>
		protected readonly List<IReducer<TState>> Reducers = new List<IReducer<TState>>();

		private SpinLock SpinLock = new SpinLock();
		private Timer NotificationTimer;
		private int NotificationThrottleWindowMs;
		private DateTime LastNotificationTime;

		/// <summary>
		/// Creates a new instance
		/// </summary>
		public Feature()
		{
			State = GetInitialState();
			byte frameRate = GetMaximumNotificationsPerSecond();
			if (frameRate > 0)
				NotificationThrottleWindowMs = 1000 / frameRate;
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

		private TState _State;

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

		/// <see cref="IFeature{TState}.State"/>
		public virtual TState State
		{
			get => _State;
			protected set
			{
				SpinLock.ExecuteLocked(() =>
				{
					bool stateHasChanged = !Object.ReferenceEquals(_State, value);
					_State = value;
					if (stateHasChanged)
						TriggerStateChangedCallbacks();
				});
			}
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
			// If no throttling then notify immediately
			if (NotificationThrottleWindowMs == 0)
			{
				OnStateChanged();
				return;
			}

			int timeSinceLastNotificationMs =
				(int)(DateTime.UtcNow - LastNotificationTime).TotalMilliseconds;

			// If we are outside the render throttle window then notify immediately
			if (timeSinceLastNotificationMs > NotificationThrottleWindowMs)
			{
				OnStateChanged();
				return;
			}

			// If waiting for a previously throttled notification to execute
			// then ignore this notification request
			if (NotificationTimer != null)
				return;

			// This is the first time we've been told to notify observers
			// within the throttle window, so stop accepting requests to
			// notify observers until the window has expired, and set
			// a timer to trigger a notification at the end of that window
			// so we get a single notification at the end
			NotificationTimer = new Timer(
				callback: _ => OnStateChanged(),
				state: null,
				dueTime: NotificationThrottleWindowMs - timeSinceLastNotificationMs,
				period: 0);
		}

		private void OnStateChanged()
		{
			LastNotificationTime = DateTime.UtcNow;
			stateChanged?.Invoke(this, State);
			untypedStateChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}
