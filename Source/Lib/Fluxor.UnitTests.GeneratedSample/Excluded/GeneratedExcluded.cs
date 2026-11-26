namespace Fluxor.UnitTests.GeneratedSample.Excluded;

// A middleware makes its namespace opt-in: scanned only when AddMiddleware names the middleware
public class GeneratedMiddleware : Middleware { }

public record GeneratedExcludedAction;

[FeatureState]
public record GeneratedExcludedState(int Count)
{
	private GeneratedExcludedState() : this(0) { }
}

public static class GeneratedExcludedReducers
{
	[ReducerMethod]
	public static GeneratedExcludedState Reduce(GeneratedExcludedState state, GeneratedExcludedAction action) =>
		state with { Count = state.Count + 1 };
}
