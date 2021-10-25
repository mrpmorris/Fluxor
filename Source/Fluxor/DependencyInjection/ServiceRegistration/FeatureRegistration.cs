using Fluxor.DependencyInjection.WrapperFactories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fluxor.DependencyInjection.ServiceRegistration
{
	internal static class FeatureRegistration
	{
		public static void Register(
			IServiceCollection services,
			FeatureClassInfo[] featureClassInfos,
			FeatureStateInfo[] featureStateInfos,
			ReducerClassInfo[] reducerClassInfos,
			ReducerMethodInfo[] reducerMethodInfos)
		{
			Dictionary<Type, IGrouping<Type, ReducerClassInfo>> reducerClassInfoByStateType =
				reducerClassInfos
				.GroupBy(x => x.StateType)
				.ToDictionary(x => x.Key);

			Dictionary<Type, IGrouping<Type, ReducerMethodInfo>> reducerMethodInfoByStateType =
				reducerMethodInfos
					.GroupBy(x => x.StateType)
					.ToDictionary(x => x.Key);

			RegisterFeatureClassInfos(
				services,
				featureClassInfos,
				reducerClassInfoByStateType,
				reducerMethodInfoByStateType);

			RegisterStateInfos(
				services,
				featureStateInfos,
				reducerClassInfoByStateType,
				reducerMethodInfoByStateType);
		}

		private static void RegisterFeatureClassInfos(IServiceCollection services, FeatureClassInfo[] featureClassInfos, Dictionary<Type, IGrouping<Type, ReducerClassInfo>> reducerClassInfoByStateType, Dictionary<Type, IGrouping<Type, ReducerMethodInfo>> reducerMethodInfoByStateType)
		{
			foreach (FeatureClassInfo info in featureClassInfos)
			{
				reducerClassInfoByStateType.TryGetValue(
					info.StateType,
					out IGrouping<Type, ReducerClassInfo> reducerClassInfosForStateType);

				reducerMethodInfoByStateType.TryGetValue(
					info.StateType,
					out IGrouping<Type, ReducerMethodInfo> reducerMethodInfosForStateType);

				// Register the implementing type so we can get an instance from the service provider
				services.AddScoped(info.ImplementingType);

				// Register a factory for the feature's interface
				services.AddScoped(info.FeatureInterfaceGenericType, serviceProvider =>
				{
					// Create an instance of the implementing type
					var featureInstance =
						(IFeature)serviceProvider.GetService(info.ImplementingType);

					AddReducers(
						serviceProvider,
						featureInstance,
						reducerClassInfosForStateType,
						reducerMethodInfosForStateType);

					return featureInstance;
				});
			}
		}

		private static MethodInfo GetAddReducerMethod(Type featureImplementingType)
		{
			string addReducerMethodName = nameof(IFeature<object>.AddReducer);
			MethodInfo featureAddReducerMethodInfo =
				featureImplementingType.GetMethod(addReducerMethodName);
			return featureAddReducerMethodInfo;

		}

		private static void RegisterStateInfos(
			IServiceCollection services,
			FeatureStateInfo[] featureStateInfos,
			Dictionary<Type, IGrouping<Type, ReducerClassInfo>> reducerClassInfoByStateType,
			Dictionary<Type, IGrouping<Type, ReducerMethodInfo>> reducerMethodInfoByStateType)
		{
			foreach (FeatureStateInfo info in featureStateInfos)
			{
				reducerClassInfoByStateType.TryGetValue(
					info.StateType,
					out IGrouping<Type, ReducerClassInfo> reducerClassInfosForStateType);

				reducerMethodInfoByStateType.TryGetValue(
					info.StateType,
					out IGrouping<Type, ReducerMethodInfo> reducerMethodInfosForStateType);

				// Register a factory for the feature's interface
				services.AddScoped(info.FeatureInterfaceGenericType, serviceProvider =>
				{
					// Create an instance of the implementing type
					ConstructorInfo featureConstructor =
					info.FeatureWrapperGenericType.GetConstructor(
						new Type[] { typeof(FeatureStateInfo) });

					var featureInstance =
						(IFeature)featureConstructor.Invoke(new object[] { info });

					AddReducers(
						serviceProvider,
						featureInstance,
						reducerClassInfosForStateType,
						reducerMethodInfosForStateType);

					return featureInstance;
				});
			}
		}

		private static void AddReducers(
			IServiceProvider serviceProvider,
			IFeature featureInstance,
			IEnumerable<ReducerClassInfo> reducerClassInfosForStateType,
			IEnumerable<ReducerMethodInfo> reducerMethodInfosForStateType)
		{
			MethodInfo featureAddReducerMethodInfo = GetAddReducerMethod(featureInstance.GetType());

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
		}
	}
}
