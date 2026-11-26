using Fluxor;
using System.Threading.Tasks;

namespace Fixture;

[FeatureState]
public partial record TextState(string Text)
{
	public TextState() : this("") { }
}

public record AppendAction;

public partial class PartialHost
{
	[ReducerMethod]
	public static TextState ReduceA1(TextState state, AppendAction action) => state with { Text = state.Text + "A1;" };

	[EffectMethod]
	public Task HandleA(AppendAction action, IDispatcher dispatcher)
	{
		dispatcher.Dispatch(new Shared.Logged("A"));
		return Task.CompletedTask;
	}

	[ReducerMethod(typeof(AppendAction))]
	public TextState ReduceA2(TextState state) => state with { Text = state.Text + "A2;" };
}
