using System.Collections.Generic;
using System.Linq;

namespace Fluxor.DependencyInjection.InfoFactories
{
	internal static class MiddlewareClassesDiscovery
	{
		internal static AssemblyAndNamespace[] FindMiddlewareLocations(IEnumerable<IFluxorModule> modulesToImport)
		=>
			modulesToImport
				.SelectMany(x => x.Middlewares)
				.Select(x => new AssemblyAndNamespace(x.Assembly, x.Namespace))
				.Distinct()
				.ToArray();
	}
}
