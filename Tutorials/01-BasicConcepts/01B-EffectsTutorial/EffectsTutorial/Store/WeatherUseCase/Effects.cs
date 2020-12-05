using Fluxor;
using System.Threading.Tasks;
using BasicConcepts.EffectsTutorial.Services;
using System;

namespace BasicConcepts.EffectsTutorial.Client.Store.WeatherUseCase
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
			var forecasts = await WeatherForecastService.GetForecastAsync(DateTime.Now)
				.ConfigureAwait(false);

			dispatcher.Dispatch(new FetchDataResultAction(forecasts));
		}
	}
}
