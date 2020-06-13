using FluxorBlazorWeb.MiddlewareTutorial.Shared;
using Fluxor;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Json;

namespace FluxorBlazorWeb.MiddlewareTutorial.Client.Store.WeatherUseCase
{
    public class Effects
	{
		private readonly HttpClient Http;

		public Effects(HttpClient http)
		{
			Http = http;
		}

		[EffectMethod]
#pragma warning disable IDE0060 // Remove unused parameter
        public async Task HandleFetchDataAction(FetchDataAction action, IDispatcher dispatcher)
#pragma warning restore IDE0060 // Remove unused parameter
        {
			// var forecasts = await Http.GetJsonAsync<WeatherForecast[]>("WeatherForecast");
			var forecasts = await Http.GetFromJsonAsync<WeatherForecast[]>("WeatherForecast");
			dispatcher.Dispatch(new FetchDataResultAction(forecasts));
		}
	}
}
