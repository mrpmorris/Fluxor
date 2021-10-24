using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fluxor.DependencyInjection.DependencyScanners
{
	internal class FeatureAttributeDecoratedClassDiscovery
	{
		internal static FeatureAttributeClassInfo[] DiscoverFeatureAttributeDecoratedClasses(
			IServiceCollection serviceCollection, IEnumerable<Type> allCandidateTypes)
		{
			(Type stateType, FeatureAttribute attr)[] discoveredClasses = allCandidateTypes
				.Select(x => new
				{
					Type = x,
					FeatureAttribute = x.GetCustomAttribute<FeatureAttribute>()
				})
				.Where(x => x.FeatureAttribute != null)
				.Select(x => (x.Type, x.FeatureAttribute))
				.ToArray();

			foreach (var item in discoveredClasses)
			{
				//TODO: Resister
			}
			throw new NotImplementedException();
		}
	}
}
