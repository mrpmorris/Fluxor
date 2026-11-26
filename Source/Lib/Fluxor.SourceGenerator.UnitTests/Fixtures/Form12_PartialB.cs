using Fluxor;
using System.Threading.Tasks;

namespace Fixture;

public record OtherAction;

public partial class PartialHost
{
	[ReducerMethod(typeof(AppendAction))]
	public TextState ReduceB1(TextState state) => state with { Text = state.Text + "B1;" };

	[EffectMethod]
	public static Task HandleB(AppendAction action, IDispatcher dispatcher)
	{
		dispatcher.Dispatch(new Shared.Logged("B"));
		return Task.CompletedTask;
	}

	[ReducerMethod]
	public static TextState ReduceB2(TextState state, AppendAction action) => state with { Text = state.Text + "B2;" };
}

public partial record TextState
{
	public string Extra => "x";
}
