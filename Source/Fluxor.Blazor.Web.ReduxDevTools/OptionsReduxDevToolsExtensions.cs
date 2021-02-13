using Fluxor.Blazor.Web.ReduxDevTools;
using Fluxor.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Fluxor
{
	public static class OptionsReduxDevToolsExtensions
	{
		public static FluxorOptions UseReduxDevTools(
			this FluxorOptions options,
			Action<ReduxDevToolsMiddlewareOptions> updateReduxOptions = null)
		{
			var reduxOptions = new ReduxDevToolsMiddlewareOptions(options);
			updateReduxOptions?.Invoke(reduxOptions);

			options.AddMiddleware<ReduxDevToolsMiddleware>();
			options.Services.AddScoped<ReduxDevToolsInterop>();
			options.Services.AddScoped(_ => reduxOptions);
			options.UseRouting();
			return options;
		}
	}
}
