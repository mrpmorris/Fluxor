using Fluxor.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Immutable;
using System.Reflection;

namespace Fluxor.DependencyInjection;

/// <summary>
/// An options class for configuring Fluxor
/// </summary>
public class FluxorOptions
{
	internal ImmutableHashSet<Type> RegisteredMiddlewareTypes = ImmutableHashSet.Create<Type>();
	internal ImmutableHashSet<IFluxorModule> ModulesToImport { get; private set; } = ImmutableHashSet.Create<IFluxorModule>();
	internal StoreLifetime StoreLifetime { get; set; } = StoreLifetime.Scoped;

	/// <summary>
	/// Service collection for registering services
	/// </summary>
	public readonly IServiceCollection Services;

	/// <summary>
	/// Creates a new instance
	/// </summary>
	/// <param name="services"></param>
	public FluxorOptions(IServiceCollection services)
	{
		Services = services;
	}

	/// <summary>
	/// The Store Lifetime that should be used when registering Fluxor features/reducers/effects/middleware
	/// </summary>
	/// <param name="lifecycle">the lifecycle to use</param>
	/// <returns>Options</returns>
	/// <remarks>
	/// <list type="bullet">
	/// <item>
	/// <term>LifecycleEnum.Scoped</term>
	/// <description>(default) Create a new instance for each new request</description>
	/// </item>
	/// <item>
	/// <term>LifecycleEnum.Singleton</term>
	/// <description>Create a new instance on first request and reuse for rest of application lifetime</description>
	/// <para>
	/// NOTE: indicating Singleton should be done only for exceptional cases.
	/// For example, in MAUI/Blazor hybrid applications, the main MAUI application is a different scope then each BlazorWebView component
	/// and state needs to be shared across all scopes of the application
	/// </para>
	/// <para>
	/// This value should only be set once during the configuration of Fluxor
	/// </para>
	/// </remarks>
	public FluxorOptions WithLifetime(StoreLifetime lifecycle)
	{
		StoreLifetime = lifecycle;
		return this;
	}


	/// <summary>
	/// Enables the developer to specify a class that implements <see cref="IMiddleware"/>
	/// which should be injected into the <see cref="IStore.AddMiddleware(IMiddleware)"/> method
	/// after dependency injection has completed.
	/// </summary>
	/// <typeparam name="TMiddleware">The Middleware type</typeparam>
	/// <returns>Options</returns>
	public FluxorOptions AddMiddleware<TMiddleware>()
		where TMiddleware : IMiddleware
	{
		if (RegisteredMiddlewareTypes.Contains(typeof(TMiddleware)))
			return this;

		Services.Add(typeof(TMiddleware), this);
		Assembly assembly = typeof(TMiddleware).Assembly;
		string @namespace = typeof(TMiddleware).Namespace;

		RegisteredMiddlewareTypes = RegisteredMiddlewareTypes.Add(typeof(TMiddleware));
		return this;
	}

	/// <summary>
	/// Adds a <see cref="IFluxorModule"/> to scan.
	/// </summary>
	/// <param name="module">The module to scan.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="module"/> is null.</exception>
	public FluxorOptions AddModule(IFluxorModule module) => AddModules(module);

	/// <summary>
	/// Adds a <see cref="IFluxorModule"/> to scan.
	/// </summary>
	/// <typeparam name="TModule">The type of module to scan.</typeparam>
	/// <returns></returns>
	public FluxorOptions AddModule<TModule>() where TModule : IFluxorModule, new() =>
		AddModule(new TModule());

	public FluxorOptions ScanTypes(Type type, params Type[] additionalTypes) => this;

	/// <summary>
	/// Adds one or more <see cref="IFluxorModule"/>s to scan
	/// </summary>
	/// <param name="module">First module to scan.</param>
	/// <param name="additionalModules">Any additional modules to scan.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="module"/> is null.</exception>
	public FluxorOptions AddModules(
		IFluxorModule module,
		params IFluxorModule[] additionalModules)
	{
		if (module is null)
			throw new ArgumentNullException(nameof(module));

		ModulesToImport = ModulesToImport.Add(module);
		if (additionalModules is not null)
			foreach (IFluxorModule additionalModule in additionalModules)
				ModulesToImport = ModulesToImport.Add(additionalModule);

		return this;
	}
}
