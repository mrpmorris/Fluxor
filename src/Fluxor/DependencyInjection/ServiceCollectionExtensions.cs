using Fluxor.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluxor
{
	/// <summary>
	/// Extensions for <see cref="IServiceCollection"/>
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Adds support to Blazor for the Fluxor library
		/// </summary>
		/// <param name="serviceCollection">The service collection</param>
		/// <param name="configure">A callback used to configure options</param>
		/// <returns>The service collection</returns>
		/// <example>
		///var serviceProvider = new BrowserServiceProvider(services =&gt;
		///{
		///	services.AddFluxor(options =&gt; options
		///		.UseDependencyInjection(typeof(Program).Assembly)
		///	);
		///});
		///</example>
		public static IServiceCollection AddFluxor<TStore>(
			this IServiceCollection serviceCollection,
			Action<Options> configure = null)
			where TStore: AbstractStore
		{
			// We only use an instance so middleware can create extensions to the Options
			var options = new Options(serviceCollection);
			configure?.Invoke(options);

			// Register all middleware types with dependency injection
			foreach (Type middlewareType in Options.MiddlewareTypes)
				serviceCollection.AddScoped(middlewareType);

			IEnumerable<AssemblyScanSettings> scanIncludeList = Options.MiddlewareTypes
				.Select(t => new AssemblyScanSettings(t.Assembly, t.Namespace));

			// Scan for features and effects
			if (Options.DependencyInjectionEnabled)
			{
				DependencyScanner.Scan<TStore>(
					serviceCollection: serviceCollection,
					assembliesToScan: Options.DependencyInjectionAssembliesToScan,
					scanIncludeList: scanIncludeList);
				serviceCollection.AddScoped(typeof(IState<>), typeof(State<>));
			}

			return serviceCollection;
		}
	}
}
