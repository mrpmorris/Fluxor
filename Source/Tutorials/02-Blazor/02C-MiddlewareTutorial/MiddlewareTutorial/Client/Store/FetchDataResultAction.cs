using FluxorBlazorWeb.MiddlewareTutorial.Shared;
using System.Collections.Generic;

namespace FluxorBlazorWeb.MiddlewareTutorial.Client.Store
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
