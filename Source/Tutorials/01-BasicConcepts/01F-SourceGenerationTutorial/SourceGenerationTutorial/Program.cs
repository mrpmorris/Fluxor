using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using System;
using BasicConcepts.SourceGenerationTutorial.Services;

namespace BasicConcepts.SourceGenerationTutorial
{
	class Program
	{
		static void Main(string[] args)
		{
			var services = new ServiceCollection();
			services.AddScoped<App>();
			services.AddScoped<IWeatherForecastService, WeatherForecastService>();
			services.AddFluxor(options =>
					options
						.ImportModules(new FluxorModule(), new FluxorModule())
				);

			IServiceProvider serviceProvider = services.BuildServiceProvider();

			var app = serviceProvider.GetRequiredService<App>();
			app.Run();
		}
	}
}
