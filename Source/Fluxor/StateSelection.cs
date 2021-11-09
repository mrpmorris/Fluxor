using Fluxor.Extensions;
using System;
using System.Threading;

namespace Fluxor
{
	/// <summary>
	/// A class that is injected into Blazor components/pages that provides access
	/// to an <see cref="IFeature{TState}"/> state.
	/// </summary>
	/// <typeparam name="TState"></typeparam>
	public class StateSelection<TState, TValue> : IStateSelection<TState, TValue>
	{
		private readonly IFeature<TState> Feature;
		private bool HasSetSelector;
		private TValue PreviousValue;
		private Func<TState, TValue> Selector;
		private readonly Func<TValue, TValue, bool> HasChanged;
		private SpinLock SpinLock = new();
		private bool IsSubscribedToFeature => _stateChanged is not null;

		/// <summary>
		/// Creates an instance of the state holder
		/// </summary>
		/// <param name="feature">The feature that contains the state</param>
		public StateSelection(IFeature<TState> feature)
		{
			Feature = feature;
			Selector =
				_ => throw new InvalidOperationException($"Msust call {nameof(Select)} before accessing {nameof(Value)}");
			HasChanged = (x, y) => object.ReferenceEquals(x, y);
		}

		/// <see cref="IState{TState}.Value"/>
		public TValue Value => Selector(Feature.State);

		/// <see cref="IStateSelection{TState, TValue}"/>
		public void Select(Func<TState, TValue> selector)
		{
			if (selector == null)
				throw new ArgumentNullException(nameof(selector));

			SpinLock.ExecuteLocked(() =>
			{
				if (HasSetSelector)
					throw new InvalidOperationException("Selector has alread been set");

				Selector = selector;
				HasSetSelector = true;
			});
		}

		private EventHandler _stateChanged;
		/// <summary>
		/// Event that is executed whenever the state changes
		/// </summary>
		public event EventHandler StateChanged
		{
			add
			{
				SpinLock.ExecuteLocked(() =>
				{
					bool wasSubscribedToFeature = IsSubscribedToFeature;
					_stateChanged += value;
					if (!wasSubscribedToFeature)
						Feature.StateChanged += FeatureStateChanged;
				});
			}
			remove
			{
				SpinLock.ExecuteLocked(() =>
				{
					_stateChanged -= value;
					if (!IsSubscribedToFeature)
						Feature.StateChanged -= FeatureStateChanged;
				});
			}
		}

		private void FeatureStateChanged(object sender, EventArgs e)
		{
			TValue newValue = Selector(Feature.State);
			if (!HasChanged(newValue, PreviousValue))
				return;
			PreviousValue = newValue;

			_stateChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}
