using Fluxor;
using Microsoft.AspNetCore.Components;
using ReduxDevToolsSample.Client.Store.CounterUseCase;

namespace ReduxDevToolsSample.Client.Pages
{
	public partial class Counter
	{
		[Inject]
		private IState<CounterState> CounterState { get; set; }

		[Inject]
		public IDispatcher Dispatcher { get; set; }

		private void IncrementCount()
		{
			var action = new Store.CounterUseCase.IncrementCounterAction();
			Dispatcher.Dispatch(action);
		}
	}
}
