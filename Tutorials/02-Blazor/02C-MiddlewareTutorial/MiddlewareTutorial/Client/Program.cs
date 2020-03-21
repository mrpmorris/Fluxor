using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Fluxor;
using FluxorBlazorWeb.MiddlewareTutorial.Client.Middlewares.Logging;

namespace FluxorBlazorWeb.MiddlewareTutorial.Client
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebAssemblyHostBuilder.CreateDefault(args);
			builder.RootComponents.Add<App>("app");
			builder.Services.AddFluxor(o => o
				.ScanAssemblies(typeof(Program).Assembly)
				.UseRouting()
				.AddMiddleware<LoggingMiddleware>());

			builder.Services.AddBaseAddressHttpClient();

			await builder.Build().RunAsync();
		}
	}
}
