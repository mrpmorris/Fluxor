﻿using EffectsSample.Client.Store.WeatherUseCase;
using Fluxor;
using Microsoft.AspNetCore.Components;

namespace EffectsSample.Client.Pages
{
	public partial class FetchData
	{
		[Inject]
		private IState<Store.WeatherUseCase.State> State { get; set; }

		[Inject]
		private IDispatcher Dispatcher { get; set; }

		protected override void OnInitialized()
		{
			base.OnInitialized();
			Dispatcher.Dispatch(new FetchDataAction());
		}
	}
}
