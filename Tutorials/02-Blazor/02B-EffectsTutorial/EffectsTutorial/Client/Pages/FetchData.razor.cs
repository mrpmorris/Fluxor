using FluxorBlazorWeb.EffectsTutorial.Client.Store.WeatherUseCase;

namespace FluxorBlazorWeb.EffectsTutorial.Client.Pages
{
	public partial class FetchData
	{
		protected override void OnInitialized()
		{
			base.OnInitialized();
			Dispatcher.Dispatch(new FetchDataAction());
		}
	}
}
