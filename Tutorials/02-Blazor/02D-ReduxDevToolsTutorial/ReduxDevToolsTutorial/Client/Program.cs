using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Fluxor;
using System;
using System.Net.Http;

namespace FluxorBlazorWeb.ReduxDevToolsTutorial.Client
{
	public static class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebAssemblyHostBuilder.CreateDefault(args);
			builder.RootComponents.Add<App>("app");
			/* Replace calls to AddBaseAddressHttpClient in Program.cs with 
				builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
			*/
			//builder.Services.AddBaseAddressHttpClient();
			builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
			builder.Services.AddFluxor(o => o
				.ScanAssemblies(typeof(Program).Assembly)
				.UseReduxDevTools()
			);
			await builder.Build().RunAsync();
		}
	}
}
