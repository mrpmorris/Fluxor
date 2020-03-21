using FluxorBlazorWeb.ReduxDevToolsTutorial.Shared;
using System.Collections.Generic;

namespace FluxorBlazorWeb.ReduxDevToolsTutorial.Client.Store.WeatherUseCase
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
