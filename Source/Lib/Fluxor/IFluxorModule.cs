using System;
using System.Collections.Generic;

namespace Fluxor
{
	public interface IFluxorModule
	{
		IEnumerable<Type> Dependencies { get; }
		IEnumerable<Type> Effects { get; }
		IEnumerable<Type> Features { get; }
		IEnumerable<Type> Middlewares { get; }
		IEnumerable<Type> Reducers { get; }
	}
}
