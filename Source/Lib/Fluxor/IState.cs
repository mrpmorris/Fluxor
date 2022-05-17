namespace Fluxor
{
	/// <summary>
	/// An interface that is injected into Blazor Components / pages for accessing
	/// the state of an <see cref="IFeature{TState}"/>
	/// </summary>
	/// <typeparam name="TState">The type of the state</typeparam>
	public interface IState<TState> : IStateChangedNotifier
	{
		/// <summary>
		/// Returns the value selected from the feature state
		/// </summary>
		TState Value { get; }
	}
}
