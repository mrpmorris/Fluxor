using Fluxor.DependencyInjection;
using Fluxor.Modularlization;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Fluxor
{
	/// <summary>
	/// Extensions for <see cref="IServiceCollection"/>
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		///	<summary>
		///		Adds support to Blazor for the Fluxor library
		///	</summary>
		///	<param name="serviceCollection">The service collection</param>
		///	<param name="configure">A callback used to configure options</param>
		///	<returns>The service collection</returns>
		///	<example>
		///		services.AddFluxor(options =&gt; options
		///			.ScanAssemblies(typeof(Program).Assembly));
		///	</example>
		public static IServiceCollection AddFluxor(
			this IServiceCollection serviceCollection,
			Action<Options> configure = null)
		{
			// We only use an instance so middleware can create extensions to the Options
			var options = new Options(serviceCollection);
			configure?.Invoke(options);

			DependencyScanner.Scan(serviceCollection, options);
			serviceCollection.AddScoped<ModuleLoader>();
			serviceCollection.AddScoped(typeof(IState<>), typeof(State<>));

			return serviceCollection;
		}
	}
}
