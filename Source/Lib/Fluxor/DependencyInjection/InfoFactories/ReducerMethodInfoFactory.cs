using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;

namespace Fluxor.DependencyInjection.InfoFactories;

internal static class ReducerMethodInfoFactory
{
	internal static ReducerMethodInfo[] Create(
		IServiceCollection services,
		IEnumerable<TypeAndMethodInfo> allCandidateMethods)
	=>
		allCandidateMethods
			.Where(x => x.MethodAttribute is ReducerMethodAttribute)
			.Select(c =>
				new
				{
					HostClassType = c.Type, 
					c.MethodInfo,
					ReducerAttribute = (ReducerMethodAttribute)c.MethodAttribute
				})
			.Select(x => 
				new ReducerMethodInfo(
					x.HostClassType,
					x.ReducerAttribute,
					x.MethodInfo))
			.ToArray();
}
