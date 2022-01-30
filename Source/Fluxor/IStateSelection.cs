﻿using System;

namespace Fluxor
{
	/// <summary>
	/// An interface that is injected into Blazor Components / pages for accessing
	/// the a subset of state of an <see cref="IFeature{TState}"/>
	/// </summary>
	/// <typeparam name="TState">The type of the state</typeparam>
	/// <typeparam name="TValue">The type of the value selected from <see cref="TState"/></typeparam>
	public interface IStateSelection<TState, TValue> : IState<TValue>
	{
		/// <summary>
		/// Identifies the part of the feature state to select
		/// </summary>
		/// <param name="selector">Function to select a value from the feature state</param>
		/// <param name="valueEquals">
		///		Optional function used to check if two values are equal. 
		///		Used to determine if an update to state needs
		///		to trigger a <see cref="IStateChangedNotifier.StateChanged"/> event
		/// </param>
		/// <param name="selectedValueChanged">
		///		Optional function that is called whenever the selected value
		///		within the state changes. This is a shorthand equivalent of
		///		<see cref="IStateSelection{TState, TValue}.Changed"/>
		/// </param>
		void Select(
			Func<TState, TValue> selector,
			Func<TValue, TValue, bool> valueEquals = null,
			Action<TValue> selectedValueChanged = null);

		/// <summary>
		/// Event that is triggered whenever the selected value
		/// within the state changes
		/// </summary>
		event EventHandler<TValue> SelectedValueChanged;
	}
}
