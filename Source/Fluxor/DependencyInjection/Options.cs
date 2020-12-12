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
	public class Options
	{
		internal readonly List<Assembly> AssembliesToScan = new List<Assembly>();
		internal readonly List<Type> MiddlewareTypes = new List<Type>();
		/// <summary>
		/// Service collection for registering services
		/// </summary>
		public readonly IServiceCollection Services;

		/// <summary>
		/// Creates a new instance
		/// </summary>
		/// <param name="services"></param>
		public Options(IServiceCollection services)
		{
			Services = services;
		}

		/// <summary>
		/// Enables automatic discovery of features/effects/reducers
		/// </summary>
		/// <param name="additionalAssembliesToScan">A collection of assemblies to scan</param>
		/// <returns>Options</returns>
		public Options ScanAssemblies(Assembly assemblyToScan, params Assembly[] additionalAssembliesToScan)
		{
			if (assemblyToScan == null)
				throw new ArgumentNullException(nameof(assemblyToScan));

			var allAssemblies = new List<Assembly> { assemblyToScan };
			if (additionalAssembliesToScan != null)
				allAssemblies.AddRange(additionalAssembliesToScan);

			AssembliesToScan.AddRange(allAssemblies);
			return this;
		}

		/// <summary>
		/// Enables the developer to specify a class that implements <see cref="IMiddleware"/>
		/// which should be injected into the <see cref="IStore.AddMiddleware(IMiddleware)"/> method
		/// after dependency injection has completed.
		/// </summary>
		/// <typeparam name="TMiddleware">The Middleware type</typeparam>
		/// <returns>Options</returns>
		public Options AddMiddleware<TMiddleware>()
			where TMiddleware : IMiddleware
		{
			if (MiddlewareTypes.Contains(typeof(TMiddleware)))
				return this;

			AssembliesToScan.Add(typeof(TMiddleware).Assembly);
			MiddlewareTypes.Add(typeof(TMiddleware));
			return this;
		}
	}
}
