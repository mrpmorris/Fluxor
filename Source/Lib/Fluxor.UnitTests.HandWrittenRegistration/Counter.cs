using System.Threading.Tasks;

namespace Fluxor.UnitTests.HandWrittenRegistration;

public record Increment;
public record Reset;
public record ResetCompleted;
public record DoubleCount;
public record RequestIncrement;

[FeatureState]
public record CounterState(int Count)
{
	private CounterState() : this(0) { }
}

public class CounterReducers
{
	[ReducerMethod]
	public static CounterState ReduceIncrement(CounterState state, Increment action) =>
		state with { Count = state.Count + 1 };

	[ReducerMethod(typeof(Reset))]
	public CounterState ReduceReset(CounterState state) =>
		state with { Count = 0 };
}

public class DoubleCounterReducer : IReducer<CounterState>
{
	public CounterState Reduce(CounterState state, object action) =>
		state with { Count = state.Count * 2 };

	public bool ShouldReduceStateForAction(object action) => action is DoubleCount;
}

public class CounterEffects
{
	[EffectMethod]
	public Task HandleRequestIncrement(RequestIncrement action, IDispatcher dispatcher)
	{
		dispatcher.Dispatch(new Increment());
		return Task.CompletedTask;
	}
}

public class ResetEffect : IEffect
{
	public Task HandleAsync(object action, IDispatcher dispatcher)
	{
		dispatcher.Dispatch(new ResetCompleted());
		return Task.CompletedTask;
	}

	public bool ShouldReactToAction(object action) => action is Reset;
}

public static class CounterAuditReducers
{
	[ReducerMethod]
	public static CounterState ReduceResetCompleted(CounterState state, ResetCompleted action) => state;
}

public static class CounterAuditEffects
{
	[EffectMethod(typeof(Increment))]
	public static Task HandleIncrement(IDispatcher dispatcher) => Task.CompletedTask;
}
