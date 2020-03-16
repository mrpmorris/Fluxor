using System;

namespace Fluxor
{
	/// <summary>
	/// An interface that is injected into Blazor Components / pages for accessing
	/// the state of an <see cref="IFeature{TState}"/>
	/// </summary>
	public interface IState
	{
		/// <summary>
		/// Event that is executed whenever the state changes
		/// </summary>
		event EventHandler StateChanged;
	}

	/// <summary>
	/// An interface that is injected into Blazor Components / pages for accessing
	/// the state of an <see cref="IFeature{TState}"/>
	/// </summary>
	/// <typeparam name="TState">The type of the state</typeparam>
	public interface IState<TState> : IState
	{
		/// <summary>
		/// Returns the current state of the feature
		/// </summary>
		TState Value { get; }
		/// <summary>
		/// Event that is executed whenever the state changes
		/// </summary>
		new event EventHandler<TState> StateChanged;
	}
}
