using FluxorBlazorWeb.EffectsTutorial.Shared;
using System.Collections.Generic;

namespace FluxorBlazorWeb.EffectsTutorial.Client.Store.WeatherUseCase
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
