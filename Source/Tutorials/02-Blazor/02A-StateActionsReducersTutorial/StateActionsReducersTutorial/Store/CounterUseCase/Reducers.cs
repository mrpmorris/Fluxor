﻿using Fluxor;

namespace FluxorBlazorWeb.StateActionsReducersTutorial.Store.CounterUseCase
{
	public static class Reducers
	{
		[ReducerMethod]
		public static CounterState ReduceIncrementCounterAction(CounterState state, IncrementCounterAction action) =>
			new(clickCount: state.ClickCount + 1);
	}
}
