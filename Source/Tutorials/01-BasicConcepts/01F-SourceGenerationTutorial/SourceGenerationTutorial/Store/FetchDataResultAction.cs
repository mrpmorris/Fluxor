using BasicConcepts.SourceGenerationTutorial.Shared;
using System.Collections.Generic;

namespace BasicConcepts.SourceGenerationTutorial.Store
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
