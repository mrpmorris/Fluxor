using Fluxor;
using FluxorBlazorWeb.ReduxDevToolsTutorial.Shared;
using System.Collections.Generic;

namespace FluxorBlazorWeb.ReduxDevToolsTutorial.Client.Store.WeatherUseCase
{
	[FeatureState(Name = "Weather")]
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
