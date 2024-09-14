using Fluxor;
using BasicConcepts.MiddlewareTutorial.Services;
using BasicConcepts.MiddlewareTutorial.Store.Middlewares.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BasicConcepts.MiddlewareTutorial
{
	class Program
	{
		static void Main(string[] args)
		{
			var services = new ServiceCollection();
			services.AddScoped<App>();
			services.AddScoped<IWeatherForecastService, WeatherForecastService>();
			services.AddFluxor(o => o
				.AddModule(new GeneratedFluxorModule())
				.AddMiddleware<LoggingMiddleware>());

			IServiceProvider serviceProvider = services.BuildServiceProvider();

			var app = serviceProvider.GetRequiredService<App>();
			app.Run();
		}
	}
}
