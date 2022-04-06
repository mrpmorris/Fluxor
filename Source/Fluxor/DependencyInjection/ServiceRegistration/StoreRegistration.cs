using Fluxor.DependencyInjection.WrapperFactories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fluxor.DependencyInjection.ServiceRegistration
{
	internal static class StoreRegistration
	{
		public static void Register(
			IServiceCollection services,
			FluxorOptions options,
			FeatureClassInfo[] featureClassInfos,
			FeatureStateInfo[] featureStateInfos,
			ReducerClassInfo[] reducerClassInfos,
			ReducerMethodInfo[] reducerMethodInfos,
			EffectClassInfo[] effectClassInfos,
			EffectMethodInfo[] effectMethodInfos)
		{
			FeatureRegistration.Register(
				services,
				featureClassInfos,
				featureStateInfos,
				reducerClassInfos,
				reducerMethodInfos,
				options); ;
			ReducerClassRegistration.Register(services, reducerClassInfos, options);
			ReducerMethodRegistration.Register(services, reducerMethodInfos, options);
			EffectClassRegistration.Register(services, effectClassInfos, options);
			EffectMethodRegistration.Register(services, effectMethodInfos, options);

			services.AddRegistration<IDispatcher, Dispatcher>(options);
			// Register IActionSubscriber as an alias to Store
			services.AddRegistration<IActionSubscriber>(serviceProvider => serviceProvider.GetService<Store>(), options);
			// Register IStore as an alias to Store
			services.AddRegistration<IStore>(serviceProvider => serviceProvider.GetService<Store>(), options);

			// Register a custom factory for building IStore that will inject all effects
			services.AddRegistration(typeof(Store), serviceProvider =>
			{
				var dispatcher = serviceProvider.GetService<IDispatcher>();
				var store = new Store(dispatcher);
				foreach (FeatureClassInfo featureClassInfo in featureClassInfos)
				{
					var feature = (IFeature)serviceProvider.GetService(featureClassInfo.FeatureInterfaceGenericType);
					store.AddFeature(feature);
				}

				foreach (FeatureStateInfo featureStateInfo in featureStateInfos)
				{
					var feature = (IFeature)serviceProvider.GetService(featureStateInfo.FeatureInterfaceGenericType);
					store.AddFeature(feature);
				}

				foreach (EffectClassInfo effectClassInfo in effectClassInfos)
				{
					var effect = (IEffect)serviceProvider.GetService(effectClassInfo.ImplementingType);
					store.AddEffect(effect);
				}

				foreach (EffectMethodInfo effectMethodInfo in effectMethodInfos)
				{
					IEffect effect = EffectWrapperFactory.Create(serviceProvider, effectMethodInfo);
					store.AddEffect(effect);
				}

				foreach (Type middlewareType in options.MiddlewareTypes)
				{
					var middleware = (IMiddleware)serviceProvider.GetService(middlewareType);
					store.AddMiddleware(middleware);
				}

				return store;
			},
			options);

		}
	}
}
