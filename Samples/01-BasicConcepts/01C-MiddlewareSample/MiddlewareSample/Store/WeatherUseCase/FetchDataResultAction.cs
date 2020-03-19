using BasicConcepts.MiddlewareSample.Shared;
using System.Collections.Generic;

namespace BasicConcepts.MiddlewareSample.Client.Store.WeatherUseCase
{
	public class FetchDataResultAction
	{
		public IEnumerable<WeatherForecast> Forecasts { get; }

		public FetchDataResultAction(IEnumerable<WeatherForecast> forecasts)
		{
			Forecasts = forecasts;
		}
	}
}
