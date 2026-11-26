using Fluxor;
using System.Threading.Tasks;

namespace Fixture;

public class BaseStateFeature : Feature<Referenced.BaseState>
{
	public override string GetName() => "Base";
	protected override Referenced.BaseState GetInitialState() => new(1);
}

public record DispatchBaseAction;
public record DispatchBaseEffectAction;

public class InheritsOnly : Referenced.ReducerAndEffectBase { }

public class InheritsAndAdds : Referenced.ReducerAndEffectBase
{
	[ReducerMethod(typeof(Referenced.BaseAction))]
	public Referenced.BaseState ReduceOwn(Referenced.BaseState state) => state with { Value = state.Value - 3 };
}

public class Forwarder
{
	[EffectMethod(typeof(DispatchBaseAction))]
	public static Task Forward(IDispatcher dispatcher)
	{
		dispatcher.Dispatch(new Referenced.BaseAction());
		return Task.CompletedTask;
	}

	[EffectMethod(typeof(DispatchBaseEffectAction))]
	public static Task ForwardEffect(IDispatcher dispatcher)
	{
		dispatcher.Dispatch(new Referenced.BaseEffectAction());
		return Task.CompletedTask;
	}

	[EffectMethod]
	public static Task ForwardNote(Referenced.Note note, IDispatcher dispatcher)
	{
		dispatcher.Dispatch(new Shared.Logged(note.Entry));
		return Task.CompletedTask;
	}
}
