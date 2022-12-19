using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Fluxor;
using System.Net.Http;
using System;
using Fluxor.Blazor.Web.ReduxDevTools;

namespace FluxorBlazorWeb.ReduxDevToolsTutorial.Client
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebAssemblyHostBuilder.CreateDefault(args);
			builder.RootComponents.Add<App>("#app");

			builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

			builder.Services.AddFluxor(o =>
			{
				o.ScanAssemblies(typeof(Program).Assembly);
				o.UseRouting();
#if DEBUG
				o.UseReduxDevTools(rdt =>
				{
					rdt.Name = "Fluxor ReduxDevTools sample";
					rdt.EnableStackTrace();
				});
#endif
			});
			await builder.Build().RunAsync();
		}
	}
}
