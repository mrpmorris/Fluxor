using Fluxor;

namespace Fixture;

[FeatureState]
public record CounterState(int Count)
{
	public CounterState() : this(0) { }
}

public record IncrementAction;
public record DecrementAction;

public class CounterReducers
{
	[ReducerMethod]
	public static CounterState ReduceIncrement(CounterState state, IncrementAction action) => state with { Count = state.Count + 1 };

	[ReducerMethod]
	public static CounterState ReduceDecrement(CounterState state, DecrementAction action) => state with { Count = state.Count - 10 };

	public static CounterState NotAReducer(CounterState state, IncrementAction action) => state with { Count = 1000 };
}

public static class StaticClassReducers
{
	[ReducerMethod]
	public static CounterState ReduceIncrementAgain(CounterState state, IncrementAction action) => state with { Count = state.Count * 3 };
}
