using FluxorBlazorWeb.MiddlewareTutorial.Client.Store.CounterUseCase;

namespace FluxorBlazorWeb.MiddlewareTutorial.Client.Pages
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
