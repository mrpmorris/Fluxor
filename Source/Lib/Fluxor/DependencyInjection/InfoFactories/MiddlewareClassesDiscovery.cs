using Fluxor.DependencyInjection.Generated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fluxor.DependencyInjection.InfoFactories;

internal static class MiddlewareClassesDiscovery
{
	internal static AssemblyScanSettings[] FindMiddlewareLocations(
		IEnumerable<Assembly> assembliesToScan,
		GeneratedRegistrationCatalog catalog)
	=>
		assembliesToScan
			.SelectMany(x => GetMiddlewareTypes(x, catalog))
			.Select(x => new AssemblyScanSettings(x.Assembly, x.Namespace))
			.Distinct()
			.ToArray();

	private static IEnumerable<Type> GetMiddlewareTypes(Assembly assembly, GeneratedRegistrationCatalog catalog)
	{
		if (catalog.TryGetTypes(assembly, out Type[] generatedTypes))
			return generatedTypes.Where(t => catalog.TryGetEntry(t, out GeneratedTypeEntry entry) && entry.IsMiddleware);

		return assembly.GetTypes().Where(t => t.GetInterfaces().Any(i => i == typeof(IMiddleware)));
	}
}
