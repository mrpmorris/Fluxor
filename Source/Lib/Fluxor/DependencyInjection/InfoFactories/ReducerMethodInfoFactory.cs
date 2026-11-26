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
			.Where(c => c.GeneratedEffectMethod is null)
			.Select(c =>
				c.GeneratedReducerMethod is not null
				? new ReducerMethodInfo(c.Type, c.GeneratedReducerMethod)
				: CreateFromMethodInfo(c))
			.Where(x => x is not null)
			.ToArray();

	private static ReducerMethodInfo CreateFromMethodInfo(TypeAndMethodInfo candidate)
	{
		ReducerMethodAttribute reducerAttribute = candidate.MethodInfo.GetCustomAttribute<ReducerMethodAttribute>(false);
		return reducerAttribute is null
			? null
			: new ReducerMethodInfo(
				candidate.Type,
				reducerAttribute,
				candidate.MethodInfo);
	}
}
