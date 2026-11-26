using System.Threading.Tasks;

namespace Fluxor.UnitTests.ScannedSample.Included;

public record ScannedIncrement;

[FeatureState]
public record ScannedCounterState(int Count)
{
	private ScannedCounterState() : this(0) { }
}

public static class ScannedReducers
{
	[ReducerMethod]
	public static ScannedCounterState Reduce(ScannedCounterState state, ScannedIncrement action) =>
		state with { Count = state.Count + 1 };
}

public class ScannedEffects
{
	[EffectMethod]
	public Task OnIncrement(ScannedIncrement action, IDispatcher dispatcher) => Task.CompletedTask;
}
