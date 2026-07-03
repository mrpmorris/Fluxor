using Fluxor;
using BasicConcepts.MiddlewareTutorial.Services;
using BasicConcepts.MiddlewareTutorial.Store.Middlewares.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace BasicConcepts.MiddlewareTutorial
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var services = new ServiceCollection();
			services.AddScoped<App>();
			services.AddScoped<IWeatherForecastService, WeatherForecastService>();
			services.AddFluxor(o => o
				.ScanAssemblies(typeof(Program).Assembly)
				.AddMiddleware<LoggingMiddleware>());

			IServiceProvider serviceProvider = services.BuildServiceProvider();

			var app = serviceProvider.GetRequiredService<App>();
			await app.RunAsync();
		}
	}
}
