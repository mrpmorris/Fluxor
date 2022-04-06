using Fluxor.Blazor.Web.ReduxDevTools;
using Fluxor.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Fluxor
{
	public static class OptionsReduxDevToolsExtensions
	{
		/// <summary>
		/// Enables interaction with the brower tool Redux Dev Tools
		/// </summary>
		/// <param name="options">The current options</param>
		/// <param name="updateReduxOptions">An action to update the options</param>
		/// <returns></returns>
		public static FluxorOptions UseReduxDevTools(
			this FluxorOptions options,
			Action<ReduxDevToolsMiddlewareOptions> updateReduxOptions = null)
		{
			var reduxOptions = new ReduxDevToolsMiddlewareOptions(options);
			updateReduxOptions?.Invoke(reduxOptions);

			options.AddMiddleware<ReduxDevToolsMiddleware>();
			options.Services.AddRegistration<ReduxDevToolsInterop>(options);
			options.Services.AddRegistration(_ => reduxOptions, options);
			options.UseRouting();
			return options;
		}

		private static IServiceCollection AddRegistration<TService>(this IServiceCollection services, FluxorOptions options)
			where TService : class
		{
			return options.RegistrationLifecycle switch {
				LifecycleEnum.Scoped => services.AddScoped<TService>(),
				LifecycleEnum.Singleton => services.AddSingleton<TService>(),
				_ => services
			};
		}

		private static IServiceCollection AddRegistration<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory, FluxorOptions options)
			where TService : class
		{
			return options.RegistrationLifecycle switch {
				LifecycleEnum.Scoped => services.AddScoped(implementationFactory),
				LifecycleEnum.Singleton => services.AddSingleton(implementationFactory),
				_ => services
			};
		}

		internal static IServiceCollection AddRegistration<TService, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory, FluxorOptions options)
			where TService : class
			where TImplementation : class, TService
		{
			return options.RegistrationLifecycle switch {
				LifecycleEnum.Scoped => services.AddScoped<TService, TImplementation>(implementationFactory),
				LifecycleEnum.Singleton => services.AddSingleton<TService, TImplementation>(implementationFactory),
				_ => services
			};
		}
	}


}
