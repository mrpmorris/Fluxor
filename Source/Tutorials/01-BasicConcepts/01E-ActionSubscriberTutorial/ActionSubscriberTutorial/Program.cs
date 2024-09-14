using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BasicConcepts.ActionSubscriberTutorial
{
	class Program
	{
		static void Main(string[] args)
		{
			var services = new ServiceCollection();
			services.AddScoped<App>();
			services.AddFluxor(o => o
				.AddModule(new GeneratedFluxorModule())
			);

			IServiceProvider serviceProvider = services.BuildServiceProvider();

			var app = serviceProvider.GetRequiredService<App>();
			app.Run();
		}
	}
}
