using Fluxor.DependencyInjection;
using Fluxor.Extensions;
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
		///	<param name="services">The service collection</param>
		///	<param name="configure">A callback used to configure options</param>
		///	<returns>The service collection</returns>
		///	<example>
		///		services.AddFluxor(options =&gt; options
		///			.ScanAssemblies(typeof(Program).Assembly));
		///	</example>
		public static IServiceCollection AddFluxor(
			this IServiceCollection services,
			Action<FluxorOptions> configure = null)
		{
			// We only use an instance so middleware can create extensions to the Options
			var options = new FluxorOptions(services);
			configure?.Invoke(options);

			// Register all middleware types with dependency injection
			foreach (Type middlewareType in options.MiddlewareTypes)
				services.Add(middlewareType, options);

			IEnumerable<AssemblyScanSettings> scanIncludeList = options.MiddlewareTypes
				.Select(t => new AssemblyScanSettings(t.Assembly, t.Namespace));

			ReflectionScanner.Scan(
				options: options,
				services: services,
				assembliesToScan: options.AssembliesToScan,
				typesToScan: options.TypesToScan,
				scanIncludeList: scanIncludeList);
			services.Add(typeof(IState<>), typeof(State<>), options);
			services.AddTransient(typeof(IStateSelection<,>), typeof(StateSelection<,>));

			return services;
		}

	}
}
