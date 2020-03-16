using EffectsSample.Shared;
using System;
using System.Collections.Generic;

namespace EffectsSample.Client.Store.WeatherUseCase
{
	public class State
	{
		public bool IsLoading { get; }
		public IEnumerable<WeatherForecast> Forecasts { get; }

		public State(bool isLoading, IEnumerable<WeatherForecast> forecasts)
		{
			IsLoading = isLoading;
			Forecasts = forecasts;
		}
	}
}
