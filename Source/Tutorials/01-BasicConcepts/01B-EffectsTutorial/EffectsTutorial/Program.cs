using Fluxor;
using BasicConcepts.EffectsTutorial.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BasicConcepts.EffectsTutorial
{
	class Program
	{
		static void Main(string[] args)
		{
			var services = new ServiceCollection();
			services.AddScoped<App>();
			services.AddScoped<IWeatherForecastService, WeatherForecastService>();
			services.AddFluxor(o => o
				.ScanAssemblies(typeof(Program).Assembly));

			IServiceProvider serviceProvider = services.BuildServiceProvider();

			var app = serviceProvider.GetRequiredService<App>();
			app.Run();
		}
	}
}
