using BasicConcepts.EffectsTutorial.Shared;
using System.Collections.Generic;

namespace BasicConcepts.EffectsTutorial.Store
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
