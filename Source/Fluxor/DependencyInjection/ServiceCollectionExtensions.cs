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

			serviceCollection.AddScoped<IDispatcher, Dispatcher>();
			serviceCollection.AddScoped<IModuleLoader, ModuleLoader>();
			serviceCollection.AddScoped(typeof(IState<>), typeof(State<>));
			serviceCollection.AddScoped(sp => CreateStore(sp, options));
			return serviceCollection;
		}

		private static IStore CreateStore(IServiceProvider sp, Options options)
		{
			var dispatcher = sp.GetRequiredService<IDispatcher>();
			var store = new Store(dispatcher);
			var moduleLoader = sp.GetRequiredService<IModuleLoader>();
			moduleLoader.Load(store, options.AssembliesToScan, options.MiddlewareTypes);
			return store;
		}
	}
}
