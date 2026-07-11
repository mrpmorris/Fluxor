using MauiReactor;
using System.Threading.Tasks;

namespace Fluxor.Reactor.Maui.Middlewares.Routing;

internal class Effects
{
	[EffectMethod]
	public async Task HandleGoActionAsync(GoAction action, IDispatcher _)
	{
		await Microsoft.Maui.Controls.Shell.Current.GoToAsync(action.NewUri);
	}

	[EffectMethod]
	public async Task HandleGoActionAsync<P>(GoAction<P> action, IDispatcher _) where P : new()
	{
		await Microsoft.Maui.Controls.Shell.Current.GoToAsync(action.NewUri, action.PropsInitializer);
	}
}
