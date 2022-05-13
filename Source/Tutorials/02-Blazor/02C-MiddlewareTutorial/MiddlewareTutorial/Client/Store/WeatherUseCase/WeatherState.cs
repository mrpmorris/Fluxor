using Fluxor;
using FluxorBlazorWeb.MiddlewareTutorial.Shared;
using System.Collections.Generic;

namespace FluxorBlazorWeb.MiddlewareTutorial.Client.Store.WeatherUseCase
{
	[FeatureState]
	public class WeatherState
	{
		public bool IsLoading { get; }
		public IEnumerable<WeatherForecast> Forecasts { get; }

		private WeatherState() { }
		public WeatherState(bool isLoading, IEnumerable<WeatherForecast> forecasts)
		{
			IsLoading = isLoading;
			Forecasts = forecasts;
		}
	}
}
