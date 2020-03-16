using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor.Hosting;
using Fluxor;

namespace EffectsSample.Client
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebAssemblyHostBuilder.CreateDefault(args);
			builder.RootComponents.Add<App>("app");
			builder.Services.AddFluxor(o => o
				.UseDependencyInjection(typeof(Program).Assembly)
			);
			await builder.Build().RunAsync();
		}
	}
}
