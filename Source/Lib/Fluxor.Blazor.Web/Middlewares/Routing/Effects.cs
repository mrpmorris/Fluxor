﻿using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Fluxor.Blazor.Web.Middlewares.Routing;

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
		string fullUri = NavigationManager.ToAbsoluteUri(action.NewUri).AbsoluteUri;
		if (action.ForceLoad || !UrlComparer.AreEqual(fullUri, NavigationManager.Uri))
		{
			// Only navigate if we are not already at the URI specified,
			// or if we have been told to do a proper page reload (ForceLoad)
			NavigationManager.NavigateTo(action.NewUri, action.ForceLoad);
		}
		return Task.CompletedTask;
	}
}
