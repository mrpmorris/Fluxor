using Fluxor;
using System.Threading.Tasks;
using System;
using BasicConcepts.SourceGenerationTutorial.Store;
using BasicConcepts.SourceGenerationTutorial.Services;

namespace BasicConcepts.SourceGenerationTutorial.Store.WeatherUseCase
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
