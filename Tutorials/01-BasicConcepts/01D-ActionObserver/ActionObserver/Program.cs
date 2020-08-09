using BasicConcepts.ActionObserver.Services;
using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BasicConcepts.ActionObserver
{
	class Program
	{
		static void Main(string[] args)
		{
			var services = new ServiceCollection();
			services.AddScoped<App>();
			services.AddScoped<IApiService, ApiService>();
			services.AddFluxor(o => o
				.ScanAssemblies(typeof(Program).Assembly));

			IServiceProvider serviceProvider = services.BuildServiceProvider();

			var app = serviceProvider.GetRequiredService<App>();
			app.Run();
		}
	}
}
