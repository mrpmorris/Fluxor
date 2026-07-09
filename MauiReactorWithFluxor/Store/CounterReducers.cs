using Fluxor;

public static class CounterReducers 
{
    [ReducerMethod(typeof(CounterIncrementAction))]
    public static CounterState OnIncrement(CounterState state) 
    {
        return state with
        {
            CurrentCount = state.CurrentCount + 1
        };
    }
}
