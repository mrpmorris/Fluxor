using FluxorBlazorWeb.ReduxDevToolsTutorial.Client.Store.CounterUseCase;

namespace FluxorBlazorWeb.ReduxDevToolsTutorial.Client.Pages
{
	public partial class Counter
	{
		private void IncrementCount()
		{
			var action = new IncrementCounterAction();
			Dispatcher.Dispatch(action);
		}
	}
}
