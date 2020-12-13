using Fluxor.Extensions;
using System;

namespace Fluxor
{
	// This class exists because we cannot register IFeatures for dependency injection
	// after the DI container has been created, so we need to look up the feature in
	// the store instead
	internal class FeatureWrapper<TState> : IFeature<TState>
	{
		private IFeature<TState> Feature;

		public void Initialize(IStore store)
		{
			if (store == null)
				throw new ArgumentNullException(nameof(store));

			Feature = store.GetFeatureByStateType<TState>();
			Feature.Initialize(store);
		}

		public TState State => Feature.State;

		public byte MaximumStateChangedNotificationsPerSecond
		{
			get => Feature.MaximumStateChangedNotificationsPerSecond;
			set
			{
				Feature.MaximumStateChangedNotificationsPerSecond = value;
			}
		}

		public event EventHandler<TState> StateChanged
		{
			add { Feature.StateChanged += value; }
			remove { Feature.StateChanged -= value; }
		}

		event EventHandler IFeature.StateChanged
		{
			add { (Feature as IFeature).StateChanged += value; }
			remove { (Feature as IFeature).StateChanged -= value; }
		}

		public void AddReducer(IReducer<TState> reducer)
		{
			Feature.AddReducer(reducer);
		}

		public string GetName() =>
			Feature.GetName();

		public object GetState() =>
			Feature.GetState();

		public Type GetStateType() =>
			Feature.GetStateType();

		public void ReceiveDispatchNotificationFromStore(object action)
		{
			Feature.ReceiveDispatchNotificationFromStore(action);
		}

		public void RestoreState(object value)
		{
			Feature.RestoreState(value);
		}
	}
}
