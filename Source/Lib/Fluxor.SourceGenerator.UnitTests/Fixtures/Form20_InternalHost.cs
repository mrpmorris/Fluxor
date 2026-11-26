using Fluxor;
using System.Threading.Tasks;

namespace Fixture;

[FeatureState]
internal record InternalState(int Value)
{
	public InternalState() : this(0) { }
}

public record IncrementAction;

internal class InternalHost
{
	[ReducerMethod]
	public static InternalState ReduceStatic(InternalState state, IncrementAction action) => state with { Value = state.Value + 1 };

	[ReducerMethod(typeof(IncrementAction))]
	public InternalState ReduceInstance(InternalState state) => state with { Value = state.Value * 4 };

	[EffectMethod]
	public Task Handle(IncrementAction action, IDispatcher dispatcher)
	{
		dispatcher.Dispatch(new Shared.Logged("internal effect"));
		return Task.CompletedTask;
	}
}

internal class InternalEffect : Effect<IncrementAction>
{
	public override Task HandleAsync(IncrementAction action, IDispatcher dispatcher)
	{
		dispatcher.Dispatch(new Shared.Logged("internal effect class"));
		return Task.CompletedTask;
	}
}

internal class InternalReducer : Reducer<InternalState, IncrementAction>
{
	public override InternalState Reduce(InternalState state, IncrementAction action) => state with { Value = state.Value - 1 };
}
