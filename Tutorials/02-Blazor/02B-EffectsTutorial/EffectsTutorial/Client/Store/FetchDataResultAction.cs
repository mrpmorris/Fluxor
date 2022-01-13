using FluxorBlazorWeb.EffectsTutorial.Shared;
using System.Collections.Generic;

namespace FluxorBlazorWeb.EffectsTutorial.Client.Store
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
