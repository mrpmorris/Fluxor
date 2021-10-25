using Fluxor;

namespace BasicConcepts.StateActionsReducersTutorial.Store.CounterUseCase
{
	[Feature]
	public class CounterState
	{
		public int ClickCount { get; }

		public CounterState(int clickCount)
		{
			ClickCount = clickCount;
		}

		public CounterState() { }
		//private static CounterState CreateInitialState() => new CounterState(clickCount: 0);
	}
}
