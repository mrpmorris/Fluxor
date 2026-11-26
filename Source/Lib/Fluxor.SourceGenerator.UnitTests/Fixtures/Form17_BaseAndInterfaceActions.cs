using Fluxor;
using System.Threading.Tasks;

namespace Fixture;

public interface IMarker { }
public abstract record BaseAction;
public record DerivedAction : BaseAction, IMarker;
public record MarkerOnlyAction : IMarker;
public record PlainAction;

public class Effects
{
	[EffectMethod]
	public Task HandleBase(BaseAction action, IDispatcher dispatcher)
	{
		dispatcher.Dispatch(new Shared.Logged("base " + action.GetType().Name));
		return Task.CompletedTask;
	}

	[EffectMethod]
	public static Task HandleMarker(IMarker action, IDispatcher dispatcher)
	{
		dispatcher.Dispatch(new Shared.Logged("marker " + action.GetType().Name));
		return Task.CompletedTask;
	}

	[EffectMethod(typeof(IMarker))]
	public Task HandleMarkerWithoutAction(IDispatcher dispatcher)
	{
		dispatcher.Dispatch(new Shared.Logged("marker without action"));
		return Task.CompletedTask;
	}

	[EffectMethod]
	public static Task HandleObject(object action, IDispatcher dispatcher)
	{
		if (action is PlainAction)
			dispatcher.Dispatch(new Shared.Logged("object " + action.GetType().Name));
		return Task.CompletedTask;
	}
}
