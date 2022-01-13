using FluxorBlazorWeb.EffectsTutorial.Client.Store.CounterUseCase;
using Fluxor;
using Microsoft.AspNetCore.Components;
using FluxorBlazorWeb.EffectsTutorial.Client.Store;

namespace FluxorBlazorWeb.EffectsTutorial.Client.Pages
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
