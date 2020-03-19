using FluxorConcepts.EffectsSample.Shared;
using System.Collections.Generic;

namespace FluxorConcepts.EffectsSample.Client.Store.WeatherUseCase
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
