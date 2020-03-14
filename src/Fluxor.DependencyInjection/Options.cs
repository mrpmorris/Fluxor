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
		internal static bool DependencyInjectionEnabled { get; private set; }
		internal static AssemblyScanSettings[] DependencyInjectionAssembliesToScan { get; private set; } = new AssemblyScanSettings[0];
		internal static Type[] MiddlewareTypes = new Type[0];
		/// <summary>
		/// Service collection for registering services
		/// </summary>
		public readonly IServiceCollection ServiceCollection;

		/// <summary>
		/// Creates a new instance
		/// </summary>
		/// <param name="serviceCollection"></param>
		public Options(IServiceCollection serviceCollection)
		{
			ServiceCollection = serviceCollection;
		}

		/// <summary>
		/// Enables automatic discovery of features/effects/reducers
		/// </summary>
		/// <param name="assembliesToScan">A collection of assemblies to scan</param>
		/// <returns>Options</returns>
		public Options UseDependencyInjection(params Assembly[] assembliesToScan)
		{
			if (assembliesToScan == null || assembliesToScan.Length == 0)
				throw new ArgumentNullException(nameof(assembliesToScan));

			var newAssembliesToScan = assembliesToScan.Select(x => new AssemblyScanSettings(x)).ToList();
			newAssembliesToScan.AddRange(DependencyInjectionAssembliesToScan);
			DependencyInjectionEnabled = true;
			DependencyInjectionAssembliesToScan = newAssembliesToScan.ToArray();

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
			if (Array.IndexOf(MiddlewareTypes, typeof(TMiddleware)) > -1)
				return this;

			ServiceCollection.AddScoped(typeof(TMiddleware));
			Assembly assembly = typeof(TMiddleware).Assembly;
			string @namespace = typeof(TMiddleware).Namespace;

			DependencyInjectionAssembliesToScan = new List<AssemblyScanSettings>(DependencyInjectionAssembliesToScan)
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
