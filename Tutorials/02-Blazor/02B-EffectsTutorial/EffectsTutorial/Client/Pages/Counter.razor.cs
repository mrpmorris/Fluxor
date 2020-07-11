using FluxorBlazorWeb.EffectsTutorial.Client.Store.CounterUseCase;

namespace FluxorBlazorWeb.EffectsTutorial.Client.Pages
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
