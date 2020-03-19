using FluxorConcepts.MiddlewareSample.Shared;
using System.Collections.Generic;

namespace FluxorConcepts.MiddlewareSample.Client.Store.WeatherUseCase
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
