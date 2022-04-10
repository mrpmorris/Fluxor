using Fluxor.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Fluxor.Extensions
{
	public static class IServiceCollectionExtensions
	{
		public static IServiceCollection Add
			(this IServiceCollection services, Type serviceType, FluxorOptions options)
		{
			return options.StoreLifetime switch {
				StoreLifetime.Scoped => services.AddScoped(serviceType),
				StoreLifetime.Singleton => services.AddSingleton(serviceType),
				_ => services
			};
		}

		public static IServiceCollection Add(this IServiceCollection services, Type serviceType, Type implementationType, FluxorOptions options)
		{
			return options.StoreLifetime switch {
				StoreLifetime.Scoped => services.AddScoped(serviceType, implementationType),
				StoreLifetime.Singleton => services.AddSingleton(serviceType, implementationType),
				_ => services
			};
		}

		public static IServiceCollection Add(this IServiceCollection services, Type serviceType, Func<IServiceProvider, object> implementationFactory, FluxorOptions options)
		{
			return options.StoreLifetime switch {
				StoreLifetime.Scoped => services.AddScoped(serviceType, implementationFactory),
				StoreLifetime.Singleton => services.AddSingleton(serviceType, implementationFactory),
				_ => services
			};
		}

		public static IServiceCollection Add<TService, TImplementation>(this IServiceCollection services, FluxorOptions options)
			where TService : class
			where TImplementation : class, TService
		{
			return options.StoreLifetime switch {
				StoreLifetime.Scoped => services.AddScoped<TService, TImplementation>(),
				StoreLifetime.Singleton => services.AddSingleton<TService, TImplementation>(),
				_ => services
			};
		}

		public static IServiceCollection Add<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory, FluxorOptions options)
			where TService : class
		{
			return options.StoreLifetime switch {
				StoreLifetime.Scoped => services.AddScoped(implementationFactory),
				StoreLifetime.Singleton => services.AddSingleton(implementationFactory),
				_ => services
			};
		}

		public static IServiceCollection Add<TService>(this IServiceCollection services, FluxorOptions options)
			where TService : class
		{
			return options.StoreLifetime switch {
				StoreLifetime.Scoped => services.AddScoped<TService>(),
				StoreLifetime.Singleton => services.AddSingleton<TService>(),
				_ => services
			};
		}

		public static IServiceCollection Add<TService, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory, FluxorOptions options)
			where TService : class
			where TImplementation : class, TService
		{
			return options.StoreLifetime switch {
				StoreLifetime.Scoped => services.AddScoped<TService, TImplementation>(implementationFactory),
				StoreLifetime.Singleton => services.AddSingleton<TService, TImplementation>(implementationFactory),
				_ => services
			};
		}

	}
}
