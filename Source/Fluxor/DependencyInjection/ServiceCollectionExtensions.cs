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
			Action<FluxorOptions> configure = null)
		{
			// We only use an instance so middleware can create extensions to the Options
			var options = new FluxorOptions(serviceCollection);
			configure?.Invoke(options);

			// Register all middleware types with dependency injection
			foreach (Type middlewareType in options.MiddlewareTypes)
				serviceCollection.AddScoped(middlewareType);

			IEnumerable<AssemblyScanSettings> scanIncludeList = options.MiddlewareTypes
				.Select(t => new AssemblyScanSettings(t.Assembly, t.Namespace));

			DependencyScanner.Scan(
				options: options,
				serviceCollection: serviceCollection,
				assembliesToScan: options.AssembliesToScan,
				scanIncludeList: scanIncludeList);
			serviceCollection.AddScoped(typeof(IState<>), typeof(State<>));

			return serviceCollection;
		}
	}
}
