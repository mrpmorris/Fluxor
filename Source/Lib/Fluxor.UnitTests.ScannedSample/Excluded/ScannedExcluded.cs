namespace Fluxor.UnitTests.ScannedSample.Excluded;

// A middleware makes its namespace opt-in: scanned only when AddMiddleware names the middleware
public class ScannedMiddleware : Middleware { }

public record ScannedExcludedAction;

[FeatureState]
public record ScannedExcludedState(int Count)
{
	private ScannedExcludedState() : this(0) { }
}

public static class ScannedExcludedReducers
{
	[ReducerMethod]
	public static ScannedExcludedState Reduce(ScannedExcludedState state, ScannedExcludedAction action) =>
		state with { Count = state.Count + 1 };
}
