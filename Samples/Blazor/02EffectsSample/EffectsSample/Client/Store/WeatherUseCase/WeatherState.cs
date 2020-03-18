using EffectsSample.Shared;
using System.Collections.Generic;

namespace FluxorBlazorWeb.EffectsSample.Client.Store.WeatherUseCase
{
	public class WeatherState
	{
		public bool IsLoading { get; }
		public IEnumerable<WeatherForecast> Forecasts { get; }

		public WeatherState(bool isLoading, IEnumerable<WeatherForecast> forecasts)
		{
			IsLoading = isLoading;
			Forecasts = forecasts;
		}
	}
}
