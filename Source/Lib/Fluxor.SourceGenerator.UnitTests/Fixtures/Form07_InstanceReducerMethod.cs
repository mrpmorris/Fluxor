using Fluxor;

namespace Fixture;

[FeatureState]
public record CounterState(int Count)
{
	public CounterState() : this(1) { }
}

public record IncrementAction;
public record ResetAction;

public class CounterReducers
{
	private readonly int Step = 5;

	[ReducerMethod(typeof(IncrementAction))]
	public CounterState ReduceIncrement(CounterState state) => state with { Count = state.Count + Step };

	[ReducerMethod(typeof(ResetAction))]
	public CounterState ReduceReset(CounterState state) => state with { Count = 0 };

	[ReducerMethod]
	public CounterState ReduceIncrementWithAction(CounterState state, IncrementAction action) => state with { Count = state.Count * 2 };
}
