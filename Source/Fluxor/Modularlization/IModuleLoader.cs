using System;
using System.Collections.Generic;
using System.Reflection;

namespace Fluxor.Modularlization
{
	// TODO: Document
	public interface IModuleLoader
	{
		void Load(
			IStore store,
			IEnumerable<Assembly> assembliesToScan,
			IEnumerable<Type> middlewareTypes);
	}
}