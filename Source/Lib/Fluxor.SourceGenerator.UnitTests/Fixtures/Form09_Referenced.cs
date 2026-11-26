using Fluxor;
using System.Threading.Tasks;

// Compiled into a second assembly that does not opt in
namespace Referenced;

public record BaseState(int Value);
public record BaseAction;
public record BaseEffectAction;
public record Note(string Entry);

public abstract class ReducerAndEffectBase
{
	[ReducerMethod]
	public static BaseState ReduceBase(BaseState state, BaseAction action) => state with { Value = state.Value + 1 };

	[ReducerMethod(typeof(BaseAction))]
	public virtual BaseState ReduceVirtual(BaseState state) => state with { Value = state.Value * 10 };

	[EffectMethod]
	public Task HandleBase(BaseEffectAction action, IDispatcher dispatcher)
	{
		dispatcher.Dispatch(new Note("base effect on " + GetType().Name));
		return Task.CompletedTask;
	}

	[EffectMethod]
	public static Task HandleBaseStatic(BaseEffectAction action, IDispatcher dispatcher)
	{
		dispatcher.Dispatch(new Note("static base effect"));
		return Task.CompletedTask;
	}
}
