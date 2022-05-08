using Microsoft.AspNetCore.Components;
using Fluxor;
using FluxorBlazorWeb.ActionSubscriberTutorial.Client.Store.CustomerUseCases.SearchUseCases;

namespace FluxorBlazorWeb.ActionSubscriberTutorial.Client.Shared
{
	public partial class NavMenu
	{
		[Inject]
		private IState<SearchCustomersState> SearchCustomersState { get; set; } = null!;

		private bool collapseNavMenu = true;
		private string? NavMenuCssClass => collapseNavMenu ? "collapse" : null;

		private void ToggleNavMenu()
		{
			collapseNavMenu = !collapseNavMenu;
		}
	}
}