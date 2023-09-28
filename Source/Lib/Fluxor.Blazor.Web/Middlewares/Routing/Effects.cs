using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace Fluxor.Blazor.Web.Middlewares.Routing
{
	internal class Effects
	{
		private readonly NavigationManager NavigationManager;

		public Effects(NavigationManager navigationManager)
		{
			NavigationManager = navigationManager;
		}

		[EffectMethod]
		public Task HandleGoActionAsync(GoAction action, IDispatcher _)
		{
			Uri newUrl = NavigationManager.ToAbsoluteUri(action.NewUri);

			if (action.ForceLoad || !newUrl.SameAs(NavigationManager.ToAbsoluteUri(NavigationManager.Uri)))
			{
				// Only navigate if we are not already at the URI specified,
				// or if we have been told to do a proper page reload (ForceLoad)
				NavigationManager.NavigateTo(newUrl.AbsoluteUri, action.ForceLoad);
			}
			return Task.CompletedTask;
		}
	}
}
