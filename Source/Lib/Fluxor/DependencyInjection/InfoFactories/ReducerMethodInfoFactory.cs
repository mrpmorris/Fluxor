using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fluxor.DependencyInjection.InfoFactories;

internal static class ReducerMethodInfoFactory
{
	internal static ReducerMethodInfo[] Create(
		IServiceCollection services,
		IEnumerable<TypeAndMethodInfo> allCandidateMethods)
	=>
		allCandidateMethods
			.Select(c =>
				new
				{
					HostClassType = c.Type, 
					c.MethodInfo,
					ReducerAttribute = c.MethodInfo.GetCustomAttribute<ReducerMethodAttribute>(false)
				})
			.Where(x => x.ReducerAttribute is not null)
			.Select(x => new ReducerMethodInfo(
				x.HostClassType,
				x.ReducerAttribute,
				x.MethodInfo))
			.ToArray();
}
