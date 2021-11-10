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
		private Func<TValue, TValue, bool> ValueEquals;
		private SpinLock SpinLock = new();
		private bool IsSubscribedToFeature => _stateChanged is not null;

		/// <summary>
		/// Creates an instance of the state holder
		/// </summary>
		/// <param name="feature">The feature that contains the state</param>
		public StateSelection(IFeature<TState> feature)
		{
			if (feature is null)
				throw new ArgumentNullException(nameof(feature));

			Feature = feature;
			Selector =
				_ => throw new InvalidOperationException($"Must call {nameof(Select)} before accessing {nameof(Value)}");
			ValueEquals = DefaultValueEquals;
		}

		/// <see cref="IState{TState}.Value"/>
		public TValue Value => Selector(Feature.State);

		/// <see cref="IStateSelection{TState, TValue}.Select(Func{TState, TValue}, Func{TValue, TValue, bool}))"/>
		public void Select(
			Func<TState, TValue> selector,
			Func<TValue, TValue, bool> valueEquals = null)
		{
			if (selector == null)
				throw new ArgumentNullException(nameof(selector));

			SpinLock.ExecuteLocked(() =>
			{
				if (HasSetSelector)
					throw new InvalidOperationException("Selector has alread been set");

				Selector = selector;
				HasSetSelector = true;
				if (valueEquals is not null)
					ValueEquals = valueEquals;
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
			if (ValueEquals(newValue, PreviousValue))
				return;
			PreviousValue = newValue;

			_stateChanged?.Invoke(this, EventArgs.Empty);
		}

		private static bool DefaultValueEquals(TValue x, TValue y) =>
			object.ReferenceEquals(x, y)
			|| (x as IEquatable<TValue>)?.Equals(y) == true
			|| object.Equals(x, y);
	}
}
