using Fluxor;
using System.Threading.Tasks;

namespace Fixture;

[FeatureState]
public record TextState(string Text)
{
	public TextState() : this("") { }
}

public record AppendAction;

public class VirtualBase
{
	[ReducerMethod(typeof(AppendAction))]
	public virtual TextState ReduceVirtual(TextState state) => state with { Text = state.Text + "base;" };

	[ReducerMethod]
	public TextState ReduceNonVirtual(TextState state, AppendAction action) => state with { Text = state.Text + "nonvirtual;" };

	[EffectMethod(typeof(AppendAction))]
	public virtual Task HandleVirtual(IDispatcher dispatcher)
	{
		dispatcher.Dispatch(new Shared.Logged("base effect"));
		return Task.CompletedTask;
	}
}

public class OverridesWithAttribute : VirtualBase
{
	[ReducerMethod(typeof(AppendAction))]
	public override TextState ReduceVirtual(TextState state) => state with { Text = state.Text + "override;" };

	[EffectMethod(typeof(AppendAction))]
	public override Task HandleVirtual(IDispatcher dispatcher)
	{
		dispatcher.Dispatch(new Shared.Logged("override effect"));
		return Task.CompletedTask;
	}
}

public class OverridesWithoutAttribute : VirtualBase
{
	public override TextState ReduceVirtual(TextState state) => state with { Text = state.Text + "unattributed;" };

	public override Task HandleVirtual(IDispatcher dispatcher)
	{
		dispatcher.Dispatch(new Shared.Logged("unattributed effect"));
		return Task.CompletedTask;
	}
}

public class OverridesTwice : OverridesWithAttribute
{
	[ReducerMethod(typeof(AppendAction))]
	public sealed override TextState ReduceVirtual(TextState state) => state with { Text = state.Text + "twice;" };
}
