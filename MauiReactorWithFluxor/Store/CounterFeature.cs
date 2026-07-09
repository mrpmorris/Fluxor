using Fluxor;

public class CounterFeature : Feature<CounterState>
{
    public override string GetName() => "Counter";

    protected override CounterState GetInitialState()
    {
        return new CounterState 
        {
            CurrentCount = 0
        };
    }
}