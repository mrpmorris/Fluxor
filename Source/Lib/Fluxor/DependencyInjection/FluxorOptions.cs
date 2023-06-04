﻿using Fluxor.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fluxor.DependencyInjection
{
	/// <summary>
	/// An options class for configuring Fluxor
	/// </summary>
	public class FluxorOptions
	{
		internal AssemblyScanSettings[] AssembliesToScan { get; private set; } = Array.Empty<AssemblyScanSettings>();
		internal Type[] TypesToScan { get; private set; } = Array.Empty<Type>();
		internal Type[] MiddlewareTypes = Array.Empty<Type>();
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

		public FluxorOptions ImportModules(
			IFluxorModule moduleToImport,
			params IFluxorModule[] additionalModulesToImport)
		{
			return this;
		}

		public FluxorOptions ScanTypes(
			Type typeToScan,
			params Type[] additionalTypesToScan)
		{
			if (typeToScan is null)
				throw new ArgumentNullException(nameof(typeToScan));

			var allTypes = new List<Type> { typeToScan };
			if (additionalTypesToScan is not null)
				allTypes.AddRange(additionalTypesToScan);

			string genericTypeNames = string.Join(",",
				allTypes
					.Where(x => x.IsGenericTypeDefinition)
					.Select(x => x.Name));
			if (genericTypeNames != string.Empty)
				throw new InvalidOperationException($"The following types cannot be generic: {genericTypeNames}");

			TypesToScan = TypesToScan
				.Union(allTypes)
				.ToArray();

			return this;
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
		/// Enables automatic discovery of features/effects/reducers
		/// </summary>
		/// <param name="additionalAssembliesToScan">A collection of assemblies to scan</param>
		/// <returns>Options</returns>
		public FluxorOptions ScanAssemblies(
			Assembly assemblyToScan,
			params Assembly[] additionalAssembliesToScan)
		{
			if (assemblyToScan is null)
				throw new ArgumentNullException(nameof(assemblyToScan));

			var allAssemblies = new List<Assembly> { assemblyToScan };
			if (additionalAssembliesToScan is not null)
				allAssemblies.AddRange(additionalAssembliesToScan);

			var newAssembliesToScan = allAssemblies.Select(x => new AssemblyScanSettings(x)).ToList();
			newAssembliesToScan.AddRange(AssembliesToScan);
			AssembliesToScan = newAssembliesToScan.ToArray();

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
			if (Array.IndexOf(MiddlewareTypes, typeof(TMiddleware)) > -1)
				return this;

			Services.Add(typeof(TMiddleware), this);
			Assembly assembly = typeof(TMiddleware).Assembly;
			string @namespace = typeof(TMiddleware).Namespace;

			AssembliesToScan = new List<AssemblyScanSettings>(AssembliesToScan)
			{
				new AssemblyScanSettings(assembly, @namespace)
			}
			.ToArray();

			MiddlewareTypes = new List<Type>(MiddlewareTypes)
			{
				typeof(TMiddleware)
			}
			.ToArray();
			return this;
		}
	}
}
