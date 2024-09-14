using Fluxor.DependencyInjection;
using Fluxor.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluxor;

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

		// Add all registered middleware types with to dependency injection
		foreach (Type registeredMiddlewareType in options.RegisteredMiddlewareTypes)
			services.Add(registeredMiddlewareType, options);

		IEnumerable<AssemblyScanSettings> scanIncludeList = options.RegisteredMiddlewareTypes
			.Select(t => new AssemblyScanSettings(t.Assembly, t.Namespace));

		services.Add<IDispatcher, Dispatcher>(options);
		// Register IActionSubscriber as an alias to Store
		services.Add<IActionSubscriber>(serviceProvider => serviceProvider.GetService<Store>(), options);
		// Register IStore as an alias to Store
		services.Add<IStore>(serviceProvider => serviceProvider.GetService<Store>(), options);

		// TODO: PeteM - Can I still do this?
		//services.Add(typeof(IState<>), typeof(State<>), options);
		//services.AddTransient(typeof(IStateSelection<,>), typeof(StateSelection<,>));

		return services;
	}

}
