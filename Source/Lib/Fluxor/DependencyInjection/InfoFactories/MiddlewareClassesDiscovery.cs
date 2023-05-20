using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fluxor.DependencyInjection.InfoFactories
{
	internal static class MiddlewareClassesDiscovery
	{
		internal static AssemblyAndNamespace[] FindMiddlewareLocations(
			IEnumerable<Assembly> assembliesToScan,
			IEnumerable<IFluxorModule> modulesToImport)
		=>
			assembliesToScan
				.SelectMany(x => x.GetTypes().Where(t => t.GetInterfaces().Any(i => i == typeof(IMiddleware))))
				.Select(x => new AssemblyAndNamespace(x.Assembly, x.Namespace))
				.Union
				(
					modulesToImport.SelectMany(x => x.Middlewares).Select(x => new AssemblyAndNamespace(x.Assembly, x.Namespace))
				)
				.Distinct()
				.ToArray();
	}
}
