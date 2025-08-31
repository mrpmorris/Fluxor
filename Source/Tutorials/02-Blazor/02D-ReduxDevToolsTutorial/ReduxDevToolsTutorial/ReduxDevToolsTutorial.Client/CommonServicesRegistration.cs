using Fluxor;
using Fluxor.Blazor.Web.ReduxDevTools;
using ReduxDevToolsTutorial.Client.Store.Middlewares.Logging;

namespace ReduxDevToolsTutorial.Client;

public static class CommonServicesRegistration
{
	public static void Register(IConfiguration config, IServiceCollection services)
	{
		services.AddFluxor(x => x
			.ScanAssemblies(typeof(CommonServicesRegistration).Assembly)
			.UseRouting()
			.AddMiddleware<LoggingMiddleware>()
			//.UseReduxDevTools()
		);
	}
}
