using System;

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
		void Select(Func<TState, TValue> selector);
	}
}
