using FluxorBlazorWeb.StateActionsReducersTutorial.Store.CounterUseCase;

namespace FluxorBlazorWeb.StateActionsReducersTutorial.Pages
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
