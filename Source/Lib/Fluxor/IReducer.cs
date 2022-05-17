namespace Fluxor
{
	/// <summary>
	/// Identifies a class that is used to update state based on the execution of a specific action
	/// </summary>
	/// <typeparam name="TState">The class type of the state this reducer operates on</typeparam>
	public interface IReducer<TState>
	{
		/// <summary>
		/// Takes the current state and the action dispatched and returns a new state.
		/// </summary>
		/// <param name="state">The current state</param>
		/// <param name="action">The action dispatched via the store</param>
		/// <returns>The new state based on the current state + the changes the action should cause</returns>
		TState Reduce(TState state, object action);

		/// <summary>
		/// Indicates whether or not this reducer intends to alter state based on the action
		/// </summary>
		/// <param name="action">The action being dispatched via the store</param>
		/// <returns>True if the reducer should be executed</returns>
		bool ShouldReduceStateForAction(object action);
	}
}
