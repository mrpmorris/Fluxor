using Fluxor;
using System.Threading.Tasks;

namespace Fixture;

[FeatureState]
public record TextState(string Text)
{
	public TextState() : this("") { }
}

public record AppendAction;

public class HidingBase
{
	[ReducerMethod]
	public static TextState Reduce(TextState state, AppendAction action) => state with { Text = state.Text + "base static;" };

	[ReducerMethod(typeof(AppendAction))]
	public TextState ReduceInstance(TextState state) => state with { Text = state.Text + "base instance;" };

	[EffectMethod]
	public Task Handle(AppendAction action, IDispatcher dispatcher)
	{
		dispatcher.Dispatch(new Shared.Logged("base effect"));
		return Task.CompletedTask;
	}
}

public class HidesWithAttribute : HidingBase
{
	[ReducerMethod]
	public static new TextState Reduce(TextState state, AppendAction action) => state with { Text = state.Text + "derived static;" };

	[ReducerMethod(typeof(AppendAction))]
	public new TextState ReduceInstance(TextState state) => state with { Text = state.Text + "derived instance;" };

	[EffectMethod]
	public new Task Handle(AppendAction action, IDispatcher dispatcher)
	{
		dispatcher.Dispatch(new Shared.Logged("derived effect"));
		return Task.CompletedTask;
	}
}

public class HidesWithoutAttribute : HidingBase
{
	public static new TextState Reduce(TextState state, AppendAction action) => state with { Text = state.Text + "unattributed;" };

	public new TextState ReduceInstance(TextState state) => state with { Text = state.Text + "unattributed instance;" };
}
