namespace Fluxor
{
	public static class IDispatcherExtesions
	{
		/// <summary>
		/// Dispatches an action to all features added to the store and ensures all effects with a regstered
		/// interest in the action type are notified.
		/// </summary>
		/// <typeparam name="TAction">The action type to dispatch to all features</typeparam>
		public static void Dispatch<TAction>(
			this IDispatcher dispatcher) where TAction : new() =>
			dispatcher?.Dispatch(new TAction());
	}
}
