using FluxorBlazorWeb.ReduxDevToolsTutorial.Client.Store.WeatherUseCase;

namespace FluxorBlazorWeb.ReduxDevToolsTutorial.Client.Pages
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
