namespace Fluxor
{
	/// <summary>
	/// A generic implementation of a reducer
	/// </summary>
	/// <typeparam name="TState">The state that this reducer works with</typeparam>
	/// <typeparam name="TAction">The action type this reducer responds to</typeparam>
	public abstract class Reducer<TState, TAction> : IReducer<TState>
	{
		/// <summary>
		/// <see cref="IReducer{TState}.ShouldReduceStateForAction(object)"/>
		/// </summary>
		public bool ShouldReduceStateForAction(object action) => action is TAction;

		/// <summary>
		/// Reduces state in reaction to the action dispatched via the store.
		/// </summary>
		/// <param name="state">The current state</param>
		/// <param name="action">The action dispatched via the store</param>
		/// <returns>The new state based on the current state + the changes the action should cause</returns>
		public abstract TState Reduce(TState state, TAction action);

		TState IReducer<TState>.Reduce(TState state, object action) => Reduce(state, (TAction)action);
	}
}
