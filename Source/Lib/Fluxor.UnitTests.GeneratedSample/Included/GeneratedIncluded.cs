using Fluxor.UnitTests.ScannedSample.Included;
using System.Threading.Tasks;

namespace Fluxor.UnitTests.GeneratedSample.Included;

public record GeneratedIncrement;
public record GeneratedIncrementOfScannedState;

[FeatureState]
public record GeneratedCounterState(int Count)
{
	private GeneratedCounterState() : this(0) { }
}

public static class GeneratedReducers
{
	[ReducerMethod]
	public static GeneratedCounterState Reduce(GeneratedCounterState state, GeneratedIncrement action) =>
		state with { Count = state.Count + 1 };
}

// A reducer here for a state declared in the (not opted in) scanned sample
public static class GeneratedReducersOfScannedState
{
	[ReducerMethod]
	public static ScannedCounterState Reduce(ScannedCounterState state, GeneratedIncrementOfScannedState action) =>
		state with { Count = state.Count + 10 };
}

public class GeneratedEffects
{
	[EffectMethod]
	public Task OnIncrement(GeneratedIncrement action, IDispatcher dispatcher) => Task.CompletedTask;
}

public class GeneratedEffect : IEffect
{
	public Task HandleAsync(object action, IDispatcher dispatcher) => Task.CompletedTask;
	public bool ShouldReactToAction(object action) => action is GeneratedIncrement;
}
