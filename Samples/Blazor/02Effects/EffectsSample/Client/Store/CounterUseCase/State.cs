namespace EffectsSample.Client.Store.CounterUseCase
{
	public class State
	{
		public int ClickCount { get; }

		public State(int clickCount)
		{
			ClickCount = clickCount;
		}
	}
}
