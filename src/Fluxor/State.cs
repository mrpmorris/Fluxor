using System;

namespace Fluxor
{
	/// <summary>
	/// A class that is injected into Blazor components/pages that provides access
	/// to an <see cref="IFeature{TState}"/> state.
	/// </summary>
	/// <typeparam name="TState"></typeparam>
	public class State<TState> : IState<TState>
	{
		private readonly IFeature<TState> Feature;

		/// <summary>
		/// Creates an instance of the state holder
		/// </summary>
		/// <param name="feature">The feature that contains the state</param>
		public State(IFeature<TState> feature)
		{
			Feature = feature;
		}

		/// <see cref="IState{TState}.Value"/>
		public TState Value => Feature.State;

		/// <summary>
		/// Event that is executed whenever the state changes
		/// </summary>
		public event EventHandler<TState> StateChanged
		{
			add { Feature.StateChanged += value; }
			remove { Feature.StateChanged -= value; }
		}

		/// <see cref="IState.Subscribe(ComponentBase)"/>
		//TODO: Replace public void Subscribe(ComponentBase subscriber) => Feature.Subscribe(subscriber);

		/// <see cref="IState.Unsubscribe(ComponentBase)"/>
		//TODO: Replace public void Unsubscribe(ComponentBase subscriber) => Feature.Unsubscribe(subscriber);
	}
}
