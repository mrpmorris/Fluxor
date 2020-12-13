using Fluxor.DependencyInjection;
using Fluxor.Modularlization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

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
			serviceCollection.AddScoped<IActionSubscriber, Store>();
			serviceCollection.AddScoped<IModuleLoader, ModuleLoader>();
			serviceCollection.AddScoped<IObjectBuilder, ObjectBuilder>();
			serviceCollection.AddScoped(typeof(IState<>), typeof(State<>));
			serviceCollection.AddScoped(typeof(IFeature<>), typeof(FeatureWrapper<>));
			serviceCollection.AddScoped(sp => CreateStore(sp, options));
			return serviceCollection;
		}

		private static IStore CreateStore(IServiceProvider sp, Options options)
		{
			var dispatcher = sp.GetRequiredService<IDispatcher>();
			var store = new Store(dispatcher);
			var objectBuilder = sp.GetRequiredService<IObjectBuilder>();
			objectBuilder.Register<IStore>(store);
			return store;
		}
	}
}
