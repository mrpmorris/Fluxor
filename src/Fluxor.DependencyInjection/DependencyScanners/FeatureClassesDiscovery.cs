using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fluxor.DependencyInjection.DependencyScanners
{
	internal static class FeatureClassesDiscovery
	{
		internal static IEnumerable<DiscoveredFeatureClass> DiscoverFeatureClasses(
			IServiceCollection serviceCollection, 
			IEnumerable<Type> allCandidateTypes,
			IEnumerable<DiscoveredReducerClass> discoveredReducerClasses,
			IEnumerable<DiscoveredReducerMethod> discoveredReducerMethods)
		{
			Dictionary<Type, IGrouping<Type, DiscoveredReducerClass>> discoveredReducerClassesByStateType = 
				discoveredReducerClasses
				.GroupBy(x => x.StateType)
				.ToDictionary(x => x.Key);

			Dictionary<Type, IGrouping<Type, DiscoveredReducerMethod>> discoveredReducerMethodsByStateType =
				discoveredReducerMethods
				.GroupBy(x => x.StateType)
				.ToDictionary(x => x.Key);

			IEnumerable<DiscoveredFeatureClass> discoveredFeatureClasses = allCandidateTypes
				.Select(t => new
				{
					ImplementingType = t,
					GenericParameterTypes = TypeHelper.GetGenericParametersForImplementedInterface(t, typeof(IFeature<>))
				})
				.Where(x => x.GenericParameterTypes != null)
				.Select(x => new DiscoveredFeatureClass(
					implementingType: x.ImplementingType,
					stateType: x.GenericParameterTypes[0]
					)
				)
				.ToList();

			foreach (DiscoveredFeatureClass discoveredFeatureClass in discoveredFeatureClasses)
			{
				discoveredReducerClassesByStateType.TryGetValue(
					discoveredFeatureClass.StateType,
					out IGrouping<Type, DiscoveredReducerClass> discoveredFeatureClassesForStateType);

				discoveredReducerMethodsByStateType.TryGetValue(
					discoveredFeatureClass.StateType,
					out IGrouping<Type, DiscoveredReducerMethod> discoveredReducerMethodsForStateType);

				RegisterFeature(
					serviceCollection,
					discoveredFeatureClass,
					discoveredFeatureClassesForStateType,
					discoveredReducerMethodsForStateType);
			}

			return discoveredFeatureClasses;
		}

		private static void RegisterFeature(
			IServiceCollection serviceCollection,
			DiscoveredFeatureClass discoveredFeatureClass,
			IEnumerable<DiscoveredReducerClass> discoveredReducerClassesForStateType,
			IEnumerable<DiscoveredReducerMethod> discoveredReducerMethodsForStateType)
		{
			string addReducerMethodName = nameof(IFeature<object>.AddReducer);
			MethodInfo featureAddReducerMethodInfo =
				discoveredFeatureClass.ImplementingType.GetMethod(addReducerMethodName);

			// Register the implementing type so we can get an instance from the service provider
			serviceCollection.AddScoped(discoveredFeatureClass.ImplementingType);

			// Register a factory for creating instance of this feature type when requested via the generic IFeature interface
			serviceCollection.AddScoped(discoveredFeatureClass.FeatureInterfaceGenericType, serviceProvider =>
			{
				// Create an instance of the implementing type
				var featureInstance = (IFeature)serviceProvider.GetService(discoveredFeatureClass.ImplementingType);

				if (discoveredReducerClassesForStateType != null)
				{

					foreach (DiscoveredReducerClass reducerClass in discoveredReducerClassesForStateType)
					{
						object reducerInstance = serviceProvider.GetService(reducerClass.ImplementingType);
						featureAddReducerMethodInfo.Invoke(featureInstance, new object[] { reducerInstance });
					}
				}

				if (discoveredReducerMethodsForStateType != null)
				{
					foreach(DiscoveredReducerMethod reducerMethod in discoveredReducerMethodsForStateType)
					{
						object reducerWrapperInstance = ReducerWrapperFactory.Create(serviceProvider, reducerMethod);
						featureAddReducerMethodInfo.Invoke(featureInstance, new object[] { reducerWrapperInstance });
					}
				}

				return featureInstance;
			});
		}

	}
}
