using Fluxor;

// Added to every fixture. Effects report that they ran by dispatching Logged, which LogReducers.Reduce
// records; that method-based reducer also shows whether the assembly took the generated path.
namespace Shared;

[FeatureState(Name = "Log")]
public record LogState(string Text)
{
	public LogState() : this("") { }
}

public record Logged(string Entry);

public class LogReducers
{
	[ReducerMethod]
	public static LogState Reduce(LogState state, Logged action) => new(state.Text + "[" + action.Entry + "]");
}
