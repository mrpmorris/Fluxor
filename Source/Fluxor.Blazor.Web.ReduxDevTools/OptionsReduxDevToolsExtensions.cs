using Fluxor.Blazor.Web.ReduxDevTools;
using Fluxor.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Fluxor
{
	public static class OptionsReduxDevToolsExtensions
	{
		public static Options UseReduxDevTools(this Options options)
		{
			options.AddMiddleware<ReduxDevToolsMiddleware>();
			options.Services.AddScoped<ReduxDevToolsInterop>();
			options.UseRouting();
			return options;
		}
	}
}
