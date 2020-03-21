namespace FluxorBlazorWeb.ReduxDevToolsTutorial.Client.Store.CounterUseCase
{
	public class CounterState
	{
		public int ClickCount { get; }

		public CounterState(int clickCount)
		{
			ClickCount = clickCount;
		}
	}
}
