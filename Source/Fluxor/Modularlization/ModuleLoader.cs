using Fluxor.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fluxor.Modularlization
{
	public class ModuleLoader
	{
		public void Load(IStore store, Assembly assemblyToScan) =>
			Load(store, new List<Assembly> { assemblyToScan });

		public void Load(IStore store, IEnumerable<Assembly> assembliesToScan)
		{
			if (store == null)
				throw new ArgumentNullException(nameof(store));
			if (assembliesToScan == null)
				throw new ArgumentNullException(nameof(assembliesToScan));
			if (!assembliesToScan.Any())
				throw new ArgumentException(
					"Must specify at least one assembly to load from",
					nameof(assembliesToScan));

			var options = new Options(null).ScanAssemblies(
				assemblyToScan: assembliesToScan.First(),
				additionalAssembliesToScan: assembliesToScan.Skip(1).ToArray());
			DependencyScanner.Scan(null, options);
		}
	}
}
