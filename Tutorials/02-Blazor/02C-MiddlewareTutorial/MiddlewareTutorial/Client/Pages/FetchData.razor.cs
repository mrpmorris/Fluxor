using FluxorBlazorWeb.MiddlewareTutorial.Client.Store.WeatherUseCase;

namespace FluxorBlazorWeb.MiddlewareTutorial.Client.Pages
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
