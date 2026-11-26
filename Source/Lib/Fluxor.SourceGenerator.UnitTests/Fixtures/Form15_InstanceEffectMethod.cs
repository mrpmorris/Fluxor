using Fluxor;
using System.Threading.Tasks;

namespace Fixture;

public record FirstAction;
public record SecondAction;

public class Effects
{
	private readonly string Prefix = "instance ";

	[EffectMethod(typeof(FirstAction))]
	public Task HandleFirst(IDispatcher dispatcher)
	{
		dispatcher.Dispatch(new Shared.Logged(Prefix + "first"));
		return Task.CompletedTask;
	}

	[EffectMethod(typeof(SecondAction))]
	public Task HandleSecond(IDispatcher dispatcher)
	{
		dispatcher.Dispatch(new Shared.Logged(Prefix + "second"));
		return Task.CompletedTask;
	}

	[EffectMethod]
	public Task HandleFirstWithAction(FirstAction action, IDispatcher dispatcher)
	{
		dispatcher.Dispatch(new Shared.Logged(Prefix + "first with action"));
		return Task.CompletedTask;
	}
}
