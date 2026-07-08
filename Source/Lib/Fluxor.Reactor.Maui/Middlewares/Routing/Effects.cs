using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace Fluxor.Reactor.Maui.Middlewares.Routing;

internal class Effects
{
	private readonly Shell Shell;

	public Effects(Shell shell)
	{
		Shell = shell;
	}

	[EffectMethod]
	public Task HandleGoActionAsync(GoAction action, IDispatcher _)
	{
		string fullUri = (new ShellNavigationState(action.NewUri)).Location.AbsoluteUri;
		if (action.ForceLoad || !UrlComparer.AreEqual(fullUri, Shell.CurrentState.Location.AbsoluteUri))
		{
			// Only navigate if we are not already at the URI specified,
			// or if we have been told to do a proper page reload (ForceLoad)
			Shell.GoToAsync(action.NewUri, action.ForceLoad);
		}
		return Task.CompletedTask;
	}
}
