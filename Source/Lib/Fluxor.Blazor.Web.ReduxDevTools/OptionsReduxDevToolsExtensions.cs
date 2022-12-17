using Fluxor.Blazor.Web.ReduxDevTools.Internal;
using Fluxor.DependencyInjection;
using Fluxor.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Fluxor.Blazor.Web.ReduxDevTools
{
	public static class OptionsReduxDevToolsExtensions
	{
		/// <summary>
		/// Enables interaction with the brower tool Redux Dev Tools
		/// </summary>
		/// <param name="options">The current options</param>
		/// <param name="updateReduxOptions">An action to update the options</param>
		/// <returns></returns>
		public static FluxorOptions UseReduxDevTools(
			this FluxorOptions options,
			Action<ReduxDevToolsMiddlewareOptions> updateReduxOptions = null)
		{
			var reduxOptions = new ReduxDevToolsMiddlewareOptions(options);
			updateReduxOptions?.Invoke(reduxOptions);

			options.AddMiddleware<ReduxDevToolsMiddleware>();
			options.Services.Add<ReduxDevToolsInterop>(options);
			options.Services.Add(_ => reduxOptions, options);
			options.UseRouting();
			return options;
		}

	}

}
