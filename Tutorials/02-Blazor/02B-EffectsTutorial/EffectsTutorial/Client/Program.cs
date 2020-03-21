using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor.Hosting;
using Fluxor;

namespace FluxorBlazorWeb.EffectsTutorial.Client
{
	public class Program
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
