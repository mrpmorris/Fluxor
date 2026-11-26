using Fluxor;
using System.Threading.Tasks;

namespace Fixture;

public record FirstAction;
public record SecondAction;

public class ObjectEffect : IEffect
{
	public Task HandleAsync(object action, IDispatcher dispatcher)
	{
		dispatcher.Dispatch(new Shared.Logged("object effect " + action.GetType().Name));
		return Task.CompletedTask;
	}

	public bool ShouldReactToAction(object action) => action is FirstAction or SecondAction;
}

public class TypedEffect : Effect<SecondAction>
{
	public override Task HandleAsync(SecondAction action, IDispatcher dispatcher)
	{
		dispatcher.Dispatch(new Shared.Logged("typed effect"));
		return Task.CompletedTask;
	}
}

public abstract class AbstractEffect : Effect<FirstAction>
{
}
