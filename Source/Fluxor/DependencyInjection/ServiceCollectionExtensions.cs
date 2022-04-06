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
				services.AddRegistration(middlewareType, options);

			IEnumerable<AssemblyScanSettings> scanIncludeList = options.MiddlewareTypes
				.Select(t => new AssemblyScanSettings(t.Assembly, t.Namespace));

			ReflectionScanner.Scan(
				options: options,
				services: services,
				assembliesToScan: options.AssembliesToScan,
				typesToScan: options.TypesToScan,
				scanIncludeList: scanIncludeList);
			services.AddRegistration(typeof(IState<>), typeof(State<>), options);
			services.AddTransient(typeof(IStateSelection<,>), typeof(StateSelection<,>));

			return services;
		}

		internal static IServiceCollection AddRegistration(this IServiceCollection services, Type serviceType, FluxorOptions options)
		{
			return options.RegistrationLifecycle switch {
				LifecycleEnum.Scoped => services.AddScoped(serviceType),
				LifecycleEnum.Singleton => services.AddSingleton(serviceType),
				_ => services
			};
		}

		internal static IServiceCollection AddRegistration(this IServiceCollection services, Type serviceType, Type implementationType, FluxorOptions options)
		{
			return options.RegistrationLifecycle switch {
				LifecycleEnum.Scoped => services.AddScoped(serviceType, implementationType),
				LifecycleEnum.Singleton => services.AddSingleton(serviceType, implementationType),
				_ => services
			};
		}

		internal static IServiceCollection AddRegistration(this IServiceCollection services, Type serviceType, Func<IServiceProvider, object> implementationFactory, FluxorOptions options)
		{
			return options.RegistrationLifecycle switch {
				LifecycleEnum.Scoped => services.AddScoped(serviceType, implementationFactory),
				LifecycleEnum.Singleton => services.AddSingleton(serviceType, implementationFactory),
				_ => services
			};
		}

		internal static IServiceCollection AddRegistration<TService, TImplementation>(this IServiceCollection services, FluxorOptions options)
			where TService : class
			where TImplementation : class, TService
		{
			return options.RegistrationLifecycle switch {
				LifecycleEnum.Scoped => services.AddScoped<TService, TImplementation>(),
				LifecycleEnum.Singleton => services.AddSingleton<TService, TImplementation>(),
				_ => services
			};
		}

		internal static IServiceCollection AddRegistration<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory, FluxorOptions options)
			where TService : class
		{
			return options.RegistrationLifecycle switch {
				LifecycleEnum.Scoped => services.AddScoped(implementationFactory),
				LifecycleEnum.Singleton => services.AddSingleton(implementationFactory),
				_ => services
			};
		}

	}
}
