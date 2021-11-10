namespace Fluxor
{
	/// <summary>
	/// A class that is injected into Blazor components/pages that provides access
	/// to an <see cref="IFeature{TState}"/> state.
	/// </summary>
	/// <typeparam name="TState"></typeparam>
	public class State<TState> : StateSelection<TState, TState>, IStateSelection<TState, TState>
	{
		public State(IFeature<TState> feature) : base(feature)
		{
			Select(
				x => x, // Select the state itself
				valueEquals: DefaultObjectReferenceEquals); // Compare by object reference
		}

		private static bool DefaultObjectReferenceEquals(TState x, TState y) =>
			object.ReferenceEquals(x, y);
	}
}
