using Fluxor;

namespace Fixture;

[FeatureState]
public record CounterState(int Count)
{
	public CounterState() : this(0) { }
}

public record IncrementAction;
public record DoubleAction;

public class IncrementReducer : IReducer<CounterState>
{
	public CounterState Reduce(CounterState state, object action) => state with { Count = state.Count + 1 };
	public bool ShouldReduceStateForAction(object action) => action is IncrementAction;
}

public class DoubleReducer : Reducer<CounterState, DoubleAction>
{
	public override CounterState Reduce(CounterState state, DoubleAction action) => state with { Count = state.Count * 2 };
}

public abstract class AbstractReducer : IReducer<CounterState>
{
	public CounterState Reduce(CounterState state, object action) => state with { Count = -1 };
	public bool ShouldReduceStateForAction(object action) => true;
}
