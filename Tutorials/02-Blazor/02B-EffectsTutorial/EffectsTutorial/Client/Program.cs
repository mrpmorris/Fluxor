using System.Threading.Tasks;
using Fluxor;
//using Microsoft.AspNetCore.Blazor.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace FluxorBlazorWeb.EffectsTutorial.Client
{
	public static class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebAssemblyHostBuilder.CreateDefault(args);
			
			builder.RootComponents.Add<App>("app");
			builder.Services.AddFluxor(o => o
				.ScanAssemblies(typeof(Program).Assembly)
			);
			await builder.Build().RunAsync();
		}
	}
}
