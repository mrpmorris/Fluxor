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
		/// Registers a component to be re-rendered whenever the state changes
		/// </summary>
		/// <param name="subscriber">The component that will have <see cref="ComponentBase.StateHasChanged"/> executed when the state changes</param>
		//TODO: Replace void Subscribe(ComponentBase subscriber);

		/// <summary>
		/// Stops a component from being re-rendered whenever the state changes
		/// </summary>
		/// <param name="subscriber">The component that has <see cref="ComponentBase.StateHasChanged"/> executed when the state changes</param>
		//TODO: Replace void Unsubscribe(ComponentBase subscriber);
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
		event EventHandler<TState> StateChanged;
	}
}
