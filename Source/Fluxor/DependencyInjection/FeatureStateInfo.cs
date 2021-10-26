using Fluxor.DependencyInjection.Wrappers;
using System;
using System.Reflection;

namespace Fluxor.DependencyInjection
{
	internal class FeatureStateInfo
	{
		public readonly FeatureStateAttribute FeatureStateAttribute;
		public readonly Type StateType;
		public readonly Type FeatureInterfaceGenericType;
		public readonly Type FeatureWrapperGenericType;
		public readonly Func<object> CreateInitialStateFunc;

		public FeatureStateInfo(
			FeatureStateAttribute featureStateAttribute,
			Type stateType)
		{
			if (featureStateAttribute == null)
				throw new ArgumentNullException(nameof(featureStateAttribute));
			if (stateType == null)
				throw new ArgumentNullException(nameof(stateType));

			FeatureStateAttribute = featureStateAttribute;
			StateType = stateType;
			FeatureInterfaceGenericType = typeof(IFeature<>).MakeGenericType(StateType);
			FeatureWrapperGenericType = typeof(FeatureStateWrapper<>).MakeGenericType(StateType);

			if (featureStateAttribute.CreateInitialStateMethodName != null)
				CreateInitialStateFunc = CreateFactoryFromStateMethod(featureStateAttribute);
			else
				CreateInitialStateFunc = CreateFactoryFromParameterlessConstructor(featureStateAttribute);
		}

		private static T CreateStateUsingParameterlessConstructor<T>()
			where T : new()
			=> new T();

		private Func<object> CreateFactoryFromParameterlessConstructor(
			FeatureStateAttribute featureStateAttribute)
		{
			ConstructorInfo constructor = StateType.GetConstructor(Array.Empty<Type>());
			if (constructor == null)
				ThrowConstructorException();

			MethodInfo createMethod = typeof(FeatureStateInfo)
				.GetMethod(
					nameof(CreateStateUsingParameterlessConstructor),
					BindingFlags.NonPublic | BindingFlags.Static)
				.MakeGenericMethod(StateType);

			var factory = (Func<object>)createMethod.CreateDelegate(typeof(Func<object>));
			return factory;
		}

		private Func<object> CreateFactoryFromStateMethod(FeatureStateAttribute featureStateAttribute)
		{
			throw new NotImplementedException();
		}


		private void ThrowConstructorException()
		{
			throw new ArgumentException(
				message: $"{StateType.Name} must implement a public parameterless constructor if" +
				$" {nameof(FeatureStateAttribute)}.{nameof(FeatureStateAttribute.CreateInitialStateMethodName)} is null");
		}
	}
}
