using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace BasicConcepts.StateActionsReducersTutorial
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var services = new ServiceCollection();
			services.AddScoped<App>();
			services.AddFluxor(o => o
				.ScanAssemblies(typeof(Program).Assembly));

			IServiceProvider serviceProvider = services.BuildServiceProvider();

			var app = serviceProvider.GetRequiredService<App>();
			await app.RunAsync();
		}
	}
}
