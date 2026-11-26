using Fluxor.DependencyInjection.WrapperFactories;
using Fluxor.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fluxor.DependencyInjection.ServiceRegistration;

internal static class FeatureRegistration
{
	public static void Register(
		IServiceCollection services,
		FeatureClassInfo[] featureClassInfos,
		FeatureStateInfo[] featureStateInfos,
		ReducerClassInfo[] reducerClassInfos,
		ReducerMethodInfo[] reducerMethodInfos,
		FluxorOptions options)
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
			reducerMethodInfoByStateType,
			options);

		RegisterStateInfos(
			services,
			featureStateInfos,
			reducerClassInfoByStateType,
			reducerMethodInfoByStateType,
			options);
	}

	private static void RegisterFeatureClassInfos(IServiceCollection services, FeatureClassInfo[] featureClassInfos, Dictionary<Type, IGrouping<Type, ReducerClassInfo>> reducerClassInfoByStateType, Dictionary<Type, IGrouping<Type, ReducerMethodInfo>> reducerMethodInfoByStateType, FluxorOptions options)
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
			services.Add(info.ImplementingType, options);

			// Register a factory for the feature's interface
			services.Add(info.FeatureInterfaceGenericType, serviceProvider =>
			{
				// Create an instance of the implementing type
				var featureInstance =
					(IFeature)serviceProvider.GetService(info.ImplementingType);

				AddReducers(
					serviceProvider,
					featureInstance,
					info.AddReducer,
					info.GeneratedStateIndex,
					reducerClassInfosForStateType,
					reducerMethodInfosForStateType);

				return featureInstance;
			},
			options);
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
		Dictionary<Type, IGrouping<Type, ReducerMethodInfo>> reducerMethodInfoByStateType,
		FluxorOptions options)
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
			services.Add(info.FeatureInterfaceGenericType, serviceProvider =>
			{
				// Create an instance of the implementing type
				IFeature featureInstance =
					info.CreateFeature is not null
					? info.CreateFeature(info.GeneratedStateIndex)
					: CreateFeatureStateWrapper(info);

				AddReducers(
					serviceProvider,
					featureInstance,
					info.AddReducer,
					info.GeneratedStateIndex,
					reducerClassInfosForStateType,
					reducerMethodInfosForStateType);

				return featureInstance;
			},
			options);
		}
	}

	private static IFeature CreateFeatureStateWrapper(FeatureStateInfo info)
	{
		ConstructorInfo featureConstructor =
			info.FeatureWrapperGenericType.GetConstructor(
				new Type[] { typeof(FeatureStateInfo) });

		return (IFeature)featureConstructor.Invoke(new object[] { info });
	}

	/// <param name="generatedAddReducer">
	/// The generated typed AddReducer call when the feature is described by generated code, otherwise null
	/// </param>
	/// <param name="generatedStateIndex">The index generated code passes to <paramref name="generatedAddReducer"/></param>
	private static void AddReducers(
		IServiceProvider serviceProvider,
		IFeature featureInstance,
		Action<IFeature, object, int> generatedAddReducer,
		int generatedStateIndex,
		IEnumerable<ReducerClassInfo> reducerClassInfosForStateType,
		IEnumerable<ReducerMethodInfo> reducerMethodInfosForStateType)
	{
		Action<IFeature, object> addReducer =
			generatedAddReducer is not null
			? (feature, reducer) => generatedAddReducer(feature, reducer, generatedStateIndex)
			: CreateReflectionAddReducer(featureInstance.GetType());

		if (reducerClassInfosForStateType is not null)
		{
			foreach (ReducerClassInfo reducerClass in reducerClassInfosForStateType)
			{
				object reducerInstance = serviceProvider.GetService(reducerClass.ImplementingType);
				addReducer(featureInstance, reducerInstance);
			}
		}

		if (reducerMethodInfosForStateType is not null)
		{
			foreach (ReducerMethodInfo reducerMethodInfo in reducerMethodInfosForStateType)
			{
				object reducerWrapperInstance = ReducerWrapperFactory.Create(serviceProvider, reducerMethodInfo);
				addReducer(featureInstance, reducerWrapperInstance);
			}
		}
	}

	private static Action<IFeature, object> CreateReflectionAddReducer(Type featureImplementingType)
	{
		MethodInfo featureAddReducerMethodInfo = GetAddReducerMethod(featureImplementingType);
		return (feature, reducer) => featureAddReducerMethodInfo.Invoke(feature, new object[] { reducer });
	}
}
