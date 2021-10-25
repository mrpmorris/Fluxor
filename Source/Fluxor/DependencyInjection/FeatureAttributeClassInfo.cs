using Fluxor.DependencyInjection.Wrappers;
using System;
using System.Reflection;

namespace Fluxor.DependencyInjection
{
	internal class FeatureAttributeClassInfo
	{
		public readonly Type StateType;
		public readonly Type FeatureInterfaceGenericType;
		public readonly Type FeatureWrapperGenericType;
		public readonly Func<object> CreateInitialStateFunc;

		public FeatureAttributeClassInfo(
			FeatureAttribute featureAttribute,
			Type stateType)
		{
			if (featureAttribute == null)
				throw new ArgumentNullException(nameof(featureAttribute));
			if (stateType == null)
				throw new ArgumentNullException(nameof(stateType));

			StateType = stateType;
			FeatureInterfaceGenericType = typeof(IFeature<>).MakeGenericType(stateType);
			FeatureWrapperGenericType = typeof(FeatureAttributeStateWrapper<>).MakeGenericType(stateType);

			if (featureAttribute.CreateInitialStateMethodName == null)
				CreateInitialStateFunc = CreateParameterlessConstructorStateFactory(featureAttribute);
			else
				throw new NotImplementedException(nameof(FeatureAttribute.CreateInitialStateMethodName));
		}

		private static T CreateStateUsingParameterlessConstructor<T>()
			where T : new()
			=> new T();

		private Func<object> CreateParameterlessConstructorStateFactory(FeatureAttribute featureAttribute)
		{
			ConstructorInfo constructor = StateType.GetConstructor(Array.Empty<Type>());
			if (constructor == null)
				ThrowConstructorException();

			MethodInfo createMethod = typeof(FeatureAttributeClassInfo)
				.GetMethod(
					nameof(CreateStateUsingParameterlessConstructor),
					BindingFlags.NonPublic | BindingFlags.Static)
				.MakeGenericMethod(StateType);

			var factory = (Func<object>)createMethod.CreateDelegate(typeof(Func<object>));
			return factory;
		}

		private void ThrowConstructorException()
		{
			throw new ArgumentException(
				message: $"{StateType.Name} must implement a public parameterless constructor if" +
				$" {nameof(FeatureAttribute)}.{nameof(FeatureAttribute.CreateInitialStateMethodName)} is null");
		}
	}
}
