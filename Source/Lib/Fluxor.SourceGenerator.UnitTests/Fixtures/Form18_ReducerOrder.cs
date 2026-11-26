using Fluxor;

namespace Fixture;

[FeatureState]
public record TextState(string Text)
{
	public TextState() : this("") { }
}

public record AppendAction;

public class SecondHost
{
	[ReducerMethod]
	public static TextState ReduceZ(TextState state, AppendAction action) => state with { Text = state.Text + "Z;" };

	[ReducerMethod]
	public static TextState ReduceA(TextState state, AppendAction action) => state with { Text = state.Text + "A;" };
}

public class FirstHost
{
	[ReducerMethod(typeof(AppendAction))]
	public TextState ReduceM(TextState state) => state with { Text = state.Text + "M;" };

	[ReducerMethod]
	public static TextState ReduceB(TextState state, AppendAction action) => state with { Text = state.Text + "B;" };
}
