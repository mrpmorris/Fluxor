using Fluxor;
using Microsoft.AspNetCore.Components;
using FluxorBlazorWeb.ReduxDevToolsTutorial.Client.Store.CounterUseCase;
using FluxorBlazorWeb.ReduxDevToolsTutorial.Client.Store;

namespace FluxorBlazorWeb.ReduxDevToolsTutorial.Client.Pages
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
