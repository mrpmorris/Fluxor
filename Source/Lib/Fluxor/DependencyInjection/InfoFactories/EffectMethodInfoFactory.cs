using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fluxor.DependencyInjection.InfoFactories;

internal static class EffectMethodInfoFactory
{
	internal static EffectMethodInfo[] Create(
		IServiceCollection services,
		IEnumerable<TypeAndMethodInfo> allCandidateMethods)
	=>
		allCandidateMethods
			.Where(c => c.GeneratedReducerMethod is null)
			.Select(c =>
				c.GeneratedEffectMethod is not null
				? new EffectMethodInfo(c.Type, c.GeneratedEffectMethod)
				: CreateFromMethodInfo(c))
			.Where(x => x is not null)
			.ToArray();

	private static EffectMethodInfo CreateFromMethodInfo(TypeAndMethodInfo candidate)
	{
		EffectMethodAttribute effectAttribute = candidate.MethodInfo.GetCustomAttribute<EffectMethodAttribute>(false);
		return effectAttribute is null
			? null
			: new EffectMethodInfo(
				candidate.Type,
				effectAttribute,
				candidate.MethodInfo);
	}
}
