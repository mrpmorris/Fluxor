using FluxorBlazorWeb.ReduxDevToolsTutorial.Shared;
using Fluxor;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Json;

namespace FluxorBlazorWeb.ReduxDevToolsTutorial.Client.Store.WeatherUseCase
{
	public class Effects
	{
		private readonly HttpClient Http;

		public Effects(HttpClient http)
		{
			Http = http;
		}

		[EffectMethod]
		public async Task HandleFetchDataAction(FetchDataAction action, IDispatcher dispatcher)
		{
			var forecasts = await Http.GetFromJsonAsync<WeatherForecast[]>("WeatherForecast");
			dispatcher.Dispatch(new FetchDataResultAction(forecasts));
		}
	}
}
