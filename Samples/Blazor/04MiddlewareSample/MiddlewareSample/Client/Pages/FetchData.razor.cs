using MiddlewareSample.Client.Store.WeatherUseCase;
using Fluxor;
using Microsoft.AspNetCore.Components;

namespace MiddlewareSample.Client.Pages
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
