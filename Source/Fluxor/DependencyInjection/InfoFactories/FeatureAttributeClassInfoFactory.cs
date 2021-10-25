using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fluxor.DependencyInjection.InfoFactories
{
	internal class FeatureAttributeClassInfoFactory
	{
		internal static FeatureAttributeClassInfo[] Create(
			IServiceCollection services,
			IEnumerable<Type> allCandidateTypes)
		=>
			allCandidateTypes
				.Select(x => new
				{
					Type = x,
					FeatureAttribute = x.GetCustomAttribute<FeatureAttribute>()
				})
				.Where(x => x.FeatureAttribute != null)
				.Select(x => new FeatureAttributeClassInfo(x.FeatureAttribute, x.Type))
				.ToArray();
	}
}
