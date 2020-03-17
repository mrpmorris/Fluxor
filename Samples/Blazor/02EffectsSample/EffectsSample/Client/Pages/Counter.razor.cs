using Fluxor;
using Microsoft.AspNetCore.Components;

namespace EffectsSample.Client.Pages
{
	public partial class Counter
	{
		[Inject]
		private IState<Store.CounterUseCase.CounterState> State { get; set; }

		[Inject]
		public IDispatcher Dispatcher { get; set; }

		private void IncrementCount()
		{
			var action = new Store.CounterUseCase.IncrementCounterAction();
			Dispatcher.Dispatch(action);
		}
	}
}
