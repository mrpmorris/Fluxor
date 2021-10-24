using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace Fluxor.DependencyInjection.DependencyScanners
{
	internal static class FeatureClassesDiscovery
	{
		internal static FeatureClassInfo[] DiscoverFeatureClasses(
			IServiceCollection serviceCollection,
			IEnumerable<Type> allCandidateTypes,
			IEnumerable<ReducerClassInfo> discoveredReducerClasses,
			IEnumerable<ReducerMethodInfo> discoveredReducerMethods)
		{
			Dictionary<Type, IGrouping<Type, ReducerClassInfo>> discoveredReducerClassesByStateType =
				discoveredReducerClasses
				.GroupBy(x => x.StateType)
				.ToDictionary(x => x.Key);

			Dictionary<Type, IGrouping<Type, ReducerMethodInfo>> discoveredReducerMethodsByStateType =
				discoveredReducerMethods
					.GroupBy(x => x.StateType)
					.ToDictionary(x => x.Key);

			FeatureClassInfo[] discoveredFeatureClasses =
				allCandidateTypes
					.Select(t =>
						new
						{
							ImplementingType = t,
							GenericParameterTypes = TypeHelper.GetGenericParametersForImplementedInterface(t, typeof(IFeature<>))
						})
					.Where(x => x.GenericParameterTypes != null)
					.Select(x => new FeatureClassInfo(
						implementingType: x.ImplementingType,
						stateType: x.GenericParameterTypes[0]
						)
					)
					.ToArray();

			foreach (FeatureClassInfo discoveredFeatureClass in discoveredFeatureClasses)
			{
				discoveredReducerClassesByStateType.TryGetValue(
					discoveredFeatureClass.StateType,
					out IGrouping<Type, ReducerClassInfo> discoveredReducerClassesForStateType);

				discoveredReducerMethodsByStateType.TryGetValue(
					discoveredFeatureClass.StateType,
					out IGrouping<Type, ReducerMethodInfo> discoveredReducerMethodsForStateType);

				RegisterFeature(
					serviceCollection,
					discoveredFeatureClass,
					discoveredReducerClassesForStateType,
					discoveredReducerMethodsForStateType);
			}

			return discoveredFeatureClasses;
		}

		private static void RegisterFeature(
			IServiceCollection serviceCollection,
			FeatureClassInfo discoveredFeatureClass,
			IEnumerable<ReducerClassInfo> discoveredReducerClassesForStateType,
			IEnumerable<ReducerMethodInfo> discoveredReducerMethodsForStateType)
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

					foreach (ReducerClassInfo reducerClass in discoveredReducerClassesForStateType)
					{
						object reducerInstance = serviceProvider.GetService(reducerClass.ImplementingType);
						featureAddReducerMethodInfo.Invoke(featureInstance, new object[] { reducerInstance });
					}
				}

				if (discoveredReducerMethodsForStateType != null)
				{
					foreach (ReducerMethodInfo discoveredReducerMethod in discoveredReducerMethodsForStateType)
					{
						object reducerWrapperInstance = ReducerWrapperFactory.Create(serviceProvider, discoveredReducerMethod);
						featureAddReducerMethodInfo.Invoke(featureInstance, new object[] { reducerWrapperInstance });
					}
				}

				return featureInstance;
			});
		}

	}
}
