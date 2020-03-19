using Fluxor;
using FluxorConcepts.MiddlewareSample.Services;
using FluxorConcepts.MiddlewareSample.Store.Middlewares.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace FluxorConcepts.MiddlewareSample
{
	class Program
	{
		static void Main(string[] args)
		{
			var services = new ServiceCollection();
			services.AddScoped<App>();
			services.AddScoped<IWeatherForecastService, WeatherForecastService>();
			services.AddFluxor(o => o
				.ScanAssemblies(typeof(Program).Assembly)
				.AddMiddleware<LoggingMiddleware>());

			IServiceProvider serviceProvider = services.BuildServiceProvider();

			var app = serviceProvider.GetRequiredService<App>();
			app.Run();
		}
	}
}
