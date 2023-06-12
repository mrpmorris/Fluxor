using Fluxor;

namespace BasicConcepts.SourceGenerationTutorial.Store.CounterUseCase
{
	[FeatureState]
	public class CounterState
	{
		public int ClickCount { get; }

		public CounterState() { }

		public CounterState(int clickCount)
		{
			ClickCount = clickCount;
		}
	}
}
