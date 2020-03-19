using Fluxor;
using System.Threading.Tasks;
using BasicConcepts.EffectsSample.Services;
using System;

namespace BasicConcepts.EffectsSample.Client.Store.WeatherUseCase
{
	public class Effects
	{
		private readonly IWeatherForecastService WeatherForecastService;

		public Effects(IWeatherForecastService weatherForecastService)
		{
			WeatherForecastService = weatherForecastService;
		}

		[EffectMethod]
		public async Task HandleFetchDataAction(FetchDataAction action, IDispatcher dispatcher)
		{
			var forecasts = await WeatherForecastService.GetForecastAsync(DateTime.Now);
			dispatcher.Dispatch(new FetchDataResultAction(forecasts));
		}
	}
}
