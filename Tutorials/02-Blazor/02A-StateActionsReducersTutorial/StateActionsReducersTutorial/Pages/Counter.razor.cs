using FluxorBlazorWeb.StateActionsReducersTutorial.Store.CounterUseCase;
using Fluxor;
using Microsoft.AspNetCore.Components;
using FluxorBlazorWeb.StateActionsReducersTutorial.Store;

namespace FluxorBlazorWeb.StateActionsReducersTutorial.Pages
{
	public partial class Counter
	{
		[Inject]
		private IState<CounterState> CounterState { get; set; }

		[Inject]
		public IDispatcher Dispatcher { get; set; }

		private void IncrementCount()
		{
			var action = new IncrementCounterAction();
			Dispatcher.Dispatch(action);
		}
	}
}
