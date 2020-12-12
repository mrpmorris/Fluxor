using System;
using System.Collections.Generic;
using System.Linq;

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
		/// <param name="store">The store that contains the state</param>
		public State(IStore store)
		{
			if (store == null)
				throw new ArgumentNullException(nameof(store));

			IFeature[] compatibleFeatures = store.Features
				.Select(x => x.Value)
				.Where(x => x.GetStateType() == typeof(TState))
				.ToArray();

			if (compatibleFeatures.Length == 0)
				throw new KeyNotFoundException(
					$"Store does not contain a feature with state type '{typeof(TState).FullName}'");
			if (compatibleFeatures.Length > 1)
				throw new KeyNotFoundException(
					$"Store contains more than one feature with state type '{typeof(TState).FullName}'");

			Feature = (IFeature<TState>)compatibleFeatures[0];
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

		/// <summary>
		/// Event that is executed whenever the state changes
		/// </summary>
		event EventHandler IState.StateChanged
		{
			add { (Feature as IFeature).StateChanged += value; }
			remove { (Feature as IFeature).StateChanged -= value; }
		}
	}
}
