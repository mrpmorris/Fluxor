using FluxorBlazorWeb.ReduxDevToolsSample.Shared;
using System.Collections.Generic;

namespace FluxorBlazorWeb.ReduxDevToolsSample.Client.Store.WeatherUseCase
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
