using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fluxor.DependencyInjection.InfoFactories
{
	internal static class FeatureClassInfoFactory
	{
		internal static FeatureClassInfo[] Create(
			IServiceCollection serviceCollection,
			IEnumerable<Type> allCandidateTypes,
			IEnumerable<ReducerClassInfo> reducerClassInfos,
			IEnumerable<ReducerMethodInfo> reducerMethodInfos)
		{
			Dictionary<Type, IGrouping<Type, ReducerClassInfo>> reducerClassInfoByStateType =
				reducerClassInfos
				.GroupBy(x => x.StateType)
				.ToDictionary(x => x.Key);

			Dictionary<Type, IGrouping<Type, ReducerMethodInfo>> reducerMethodInfoByStateType =
				reducerMethodInfos
					.GroupBy(x => x.StateType)
					.ToDictionary(x => x.Key);

			FeatureClassInfo[] featureClassInfos =
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

			foreach (FeatureClassInfo featureClassInfo in featureClassInfos)
			{
				reducerClassInfoByStateType.TryGetValue(
					featureClassInfo.StateType,
					out IGrouping<Type, ReducerClassInfo> reducerClassInfosForStateType);

				reducerMethodInfoByStateType.TryGetValue(
					featureClassInfo.StateType,
					out IGrouping<Type, ReducerMethodInfo> reducerMethodInfosForStateType);

				RegisterFeature(
					serviceCollection,
					featureClassInfo,
					reducerClassInfosForStateType,
					reducerMethodInfosForStateType);
			}

			return featureClassInfos;
		}

		private static void RegisterFeature(
			IServiceCollection serviceCollection,
			FeatureClassInfo featureClassInfo,
			IEnumerable<ReducerClassInfo> reducerClassInfosForStateType,
			IEnumerable<ReducerMethodInfo> reducerMethodInfosForStateType)
		{
			string addReducerMethodName = nameof(IFeature<object>.AddReducer);
			MethodInfo featureAddReducerMethodInfo =
				featureClassInfo.ImplementingType.GetMethod(addReducerMethodName);

			// Register the implementing type so we can get an instance from the service provider
			serviceCollection.AddScoped(featureClassInfo.ImplementingType);

			// Register a factory for creating instance of this feature type when requested via the generic IFeature interface
			serviceCollection.AddScoped(featureClassInfo.FeatureInterfaceGenericType, serviceProvider =>
			{
				// Create an instance of the implementing type
				var featureInstance = (IFeature)serviceProvider.GetService(featureClassInfo.ImplementingType);

				if (reducerClassInfosForStateType != null)
				{

					foreach (ReducerClassInfo reducerClass in reducerClassInfosForStateType)
					{
						object reducerInstance = serviceProvider.GetService(reducerClass.ImplementingType);
						featureAddReducerMethodInfo.Invoke(featureInstance, new object[] { reducerInstance });
					}
				}

				if (reducerMethodInfosForStateType != null)
				{
					foreach (ReducerMethodInfo reducerMethodInfo in reducerMethodInfosForStateType)
					{
						object reducerWrapperInstance = ReducerWrapperFactory.Create(serviceProvider, reducerMethodInfo);
						featureAddReducerMethodInfo.Invoke(featureInstance, new object[] { reducerWrapperInstance });
					}
				}

				return featureInstance;
			});
		}

	}
}
