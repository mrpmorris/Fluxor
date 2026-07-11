using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace Fluxor.Reactor.Maui.Middlewares.Routing;

internal class Effects
{
	[EffectMethod]
	public async Task HandleGoActionAsync(GoAction action, IDispatcher _)
	{
		await Shell.Current.GoToAsync(action.NewUri);
	}
}
