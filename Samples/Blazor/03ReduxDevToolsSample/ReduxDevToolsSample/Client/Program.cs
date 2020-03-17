using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Fluxor;

namespace ReduxDevToolsSample.Client
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebAssemblyHostBuilder.CreateDefault(args);
			builder.RootComponents.Add<App>("app");
			builder.Services.AddBaseAddressHttpClient();
			builder.Services.AddFluxor(o => o
				.UseDependencyInjection(typeof(Program).Assembly)
				.UseReduxDevTools()
			);
			await builder.Build().RunAsync();
		}
	}
}
