using Fluxor;
using System.Threading.Tasks;

namespace Fixture;

public record FirstAction;
public record SecondAction;

public class Effects
{
	[EffectMethod]
	public static Task HandleFirst(FirstAction action, IDispatcher dispatcher)
	{
		dispatcher.Dispatch(new Shared.Logged("first"));
		return Task.CompletedTask;
	}

	[EffectMethod]
	public static Task HandleSecond(SecondAction action, IDispatcher dispatcher)
	{
		dispatcher.Dispatch(new Shared.Logged("second"));
		return Task.CompletedTask;
	}
}

public static class StaticClassEffects
{
	[EffectMethod]
	public static Task HandleFirstAgain(FirstAction action, IDispatcher dispatcher)
	{
		dispatcher.Dispatch(new Shared.Logged("first again"));
		return Task.CompletedTask;
	}
}
