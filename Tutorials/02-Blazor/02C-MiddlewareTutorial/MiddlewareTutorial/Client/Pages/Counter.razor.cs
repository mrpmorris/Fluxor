using Fluxor;
using Microsoft.AspNetCore.Components;
using FluxorBlazorWeb.MiddlewareTutorial.Client.Store.CounterUseCase;
using FluxorBlazorWeb.MiddlewareTutorial.Client.Store;

namespace FluxorBlazorWeb.MiddlewareTutorial.Client.Pages
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
