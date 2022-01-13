using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fluxor.DependencyInjection.InfoFactories
{
	internal class FeatureStateInfoFactory
	{
		internal static FeatureStateInfo[] Create(
			IServiceCollection services,
			IEnumerable<Type> allCandidateTypes)
		=>
			allCandidateTypes
				.Select(x => new
				{
					Type = x,
					FeatureStateAttribute = x.GetCustomAttribute<FeatureStateAttribute>()
				})
				.Where(x => x.FeatureStateAttribute is not null)
				.Select(x => new FeatureStateInfo(x.FeatureStateAttribute, x.Type))
				.ToArray();
	}
}
