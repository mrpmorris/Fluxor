using Fluxor;

namespace FluxorBlazorWeb.EffectsTutorial.Client.Store.CounterUseCase
{
	public static class Reducers
	{
        [ReducerMethod]
#pragma warning disable IDE0060 // Remove unused parameter
        public static CounterState ReduceIncrementCounterAction(CounterState state, IncrementCounterAction action)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            return new CounterState(clickCount: state.ClickCount + 1);
        }
    }
}
