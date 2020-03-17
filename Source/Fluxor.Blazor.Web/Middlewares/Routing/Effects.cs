using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace Fluxor.Blazor.Web.Middlewares.Routing
{
	internal class Effects
	{
		private readonly NavigationManager NavigationManager;

		[EffectMethod]
		public Task HandleGoActionAsync(GoAction action, IDispatcher dispatcher)
		{
			Uri fullUri = NavigationManager.ToAbsoluteUri(action.NewUri);
			if (fullUri.ToString() != NavigationManager.Uri)
			{
				// Only navigate if we are not already at the URI specified
				NavigationManager.NavigateTo(action.NewUri);
			}
			return Task.CompletedTask;
		}

		public Effects(NavigationManager navigationManager)
		{
			NavigationManager = navigationManager;
		}
	}
}
