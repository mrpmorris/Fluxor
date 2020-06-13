using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Fluxor;
using FluxorBlazorWeb.MiddlewareTutorial.Client.Middlewares.Logging;
using System.Net.Http;
using System;

namespace FluxorBlazorWeb.MiddlewareTutorial.Client
{
	public static class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebAssemblyHostBuilder.CreateDefault(args);
			builder.RootComponents.Add<App>("app");
			builder.Services.AddFluxor(o => o
				.ScanAssemblies(typeof(Program).Assembly)
				.UseRouting()
				.AddMiddleware<LoggingMiddleware>());

			/* Replace calls to AddBaseAddressHttpClient in Program.cs with 
				builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
			*/
			//builder.Services.AddBaseAddressHttpClient();
			builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

			await builder.Build().RunAsync();
		}
	}
}
