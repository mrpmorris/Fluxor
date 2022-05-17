using FluxorBlazorWeb.ReduxDevToolsTutorial.Client.Store.WeatherUseCase;
using Fluxor;
using Microsoft.AspNetCore.Components;
using FluxorBlazorWeb.ReduxDevToolsTutorial.Client.Store;

namespace FluxorBlazorWeb.ReduxDevToolsTutorial.Client.Pages
{
	public partial class FetchData
	{
		[Inject]
		private IState<WeatherState> WeatherState { get; set; }

		[Inject]
		private IDispatcher Dispatcher { get; set; }

		protected override void OnInitialized()
		{
			base.OnInitialized();
			Dispatcher.Dispatch(new FetchDataAction());
		}
	}
}
