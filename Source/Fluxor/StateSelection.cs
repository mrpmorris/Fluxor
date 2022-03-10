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
		private Action<TValue> SelectedValueChangedAction;
		private Func<TValue, TValue, bool> ValueEquals;
		private SpinLock SpinLock = new();
		private bool ShouldBeSubscribedToFeature =>
			_selectedValueChanged is not null
			|| _stateChanged is not null
			|| SelectedValueChangedAction is not null;

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
			Func<TValue, TValue, bool> valueEquals = null,
			Action<TValue> selectedValueChanged = null)
		{
			if (selector is null)
				throw new ArgumentNullException(nameof(selector));

			SpinLock.ExecuteLocked(() =>
			{
				if (HasSetSelector)
					throw new InvalidOperationException("Selector has already been set");

				bool wasSubscribedToFeature = ShouldBeSubscribedToFeature;
				Selector = selector;
				SelectedValueChangedAction = selectedValueChanged;
				HasSetSelector = true;
				if (valueEquals is not null)
					ValueEquals = valueEquals;
				PreviousValue = Value;

				if (!wasSubscribedToFeature && ShouldBeSubscribedToFeature)
					Feature.StateChanged += FeatureStateChanged;
			});
		}

		private EventHandler<TValue> _selectedValueChanged;
		/// <see cref="IStateSelection{TState, TValue}.SelectedValueChanged"/>
		public event EventHandler<TValue> SelectedValueChanged
		{
			add
			{
				SpinLock.ExecuteLocked(() =>
				{
					bool wasSubscribedToFeature = ShouldBeSubscribedToFeature;
					_selectedValueChanged += value;
					if (!wasSubscribedToFeature)
						Feature.StateChanged += FeatureStateChanged;
				});
			}
			remove
			{
				SpinLock.ExecuteLocked(() =>
				{
					_selectedValueChanged -= value;
					if (!ShouldBeSubscribedToFeature)
						Feature.StateChanged -= FeatureStateChanged;
				});
			}
		}

		private EventHandler _stateChanged;
		/// <see cref="IStateChangedNotifier.StateChanged"/>
		public event EventHandler StateChanged
		{
			add
			{
				SpinLock.ExecuteLocked(() =>
				{
					bool wasSubscribedToFeature = ShouldBeSubscribedToFeature;
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
					if (!ShouldBeSubscribedToFeature)
						Feature.StateChanged -= FeatureStateChanged;
				});
			}
		}

		private void FeatureStateChanged(object sender, EventArgs e)
		{
			if (!HasSetSelector)
				return;

			TValue newValue = Selector(Feature.State);
			if (ValueEquals(newValue, PreviousValue))
				return;
			PreviousValue = newValue;

			SelectedValueChangedAction?.Invoke(newValue);
			_selectedValueChanged?.Invoke(this, newValue);
			_stateChanged?.Invoke(this, EventArgs.Empty);
		}

		private static bool DefaultValueEquals(TValue x, TValue y) =>
			object.ReferenceEquals(x, y)
			|| (x as IEquatable<TValue>)?.Equals(y) == true
			|| object.Equals(x, y);
	}
}
