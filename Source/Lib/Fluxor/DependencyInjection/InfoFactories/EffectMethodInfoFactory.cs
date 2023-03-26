using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;

namespace Fluxor.DependencyInjection.InfoFactories
{
	internal static class EffectMethodInfoFactory
	{
		internal static EffectMethodInfo[] Create(
			IServiceCollection services,
			IEnumerable<TypeAndMethodInfo> allCandidateMethods)
		=>
			allCandidateMethods
				.Where(x => x.MethodAttribute is EffectMethodAttribute)
				.Select(c =>
					new
					{
						HostClassType = c.Type,
						c.MethodInfo,
						EffectAttribute = (EffectMethodAttribute)c.MethodAttribute
					})
				.Select(x =>
					new EffectMethodInfo(
						x.HostClassType,
						x.EffectAttribute, 
						x.MethodInfo))
				.ToArray();
	}
}
