using Fluxor;

namespace FluxorBlazorWeb.ReduxDevToolsTutorial.Client.Store.CounterUseCase
{
	[FeatureState(Name = "Counter", DebuggerBrowsable = false)]
	public class CounterState
	{
		public int ClickCount { get; }

		private CounterState() { }
		public CounterState(int clickCount)
		{
			ClickCount = clickCount;
		}
	}
}
