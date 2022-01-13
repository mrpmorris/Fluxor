using BasicConcepts.MiddlewareTutorial.Shared;
using System.Collections.Generic;

namespace BasicConcepts.MiddlewareTutorial.Store
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
