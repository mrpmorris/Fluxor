using System;
using System.Reflection;

namespace Fluxor.DependencyInjection
{
	internal class FeatureAttributeClassInfo
	{
		public readonly Type StateType;
		public readonly Type FeatureInterfaceGenericType;
		public readonly Func<IFeature> CreateFeature;

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

			if (featureAttribute.CreateInitialStateMethodName == null)
				CreateFeature = CreateParameterlessConstructorFactory(featureAttribute);
			else
				throw new NotImplementedException(nameof(FeatureAttribute.CreateInitialStateMethodName));
		}

		private static T CreateStateUsingParameterlessConstructor<T>()
			where T : new()
			=> new T();

		private Func<IFeature> CreateParameterlessConstructorFactory(FeatureAttribute featureAttribute)
		{
			ConstructorInfo constructor = StateType.GetConstructor(Array.Empty<Type>());
			if (constructor == null)
				ThrowConstructorException();

			MethodInfo createMethod = typeof(FeatureAttributeClassInfo)
				.GetMethod(
					nameof(CreateStateUsingParameterlessConstructor),
					BindingFlags.NonPublic | BindingFlags.Static)
				.MakeGenericMethod(StateType);

			return (Func<IFeature>)createMethod.CreateDelegate(typeof(Func<IFeature>));
		}

		private void ThrowConstructorException()
		{
			throw new ArgumentException(
				message: $"{StateType.Name} must implement a public parameterless constructor if" +
				$" {nameof(FeatureAttribute)}.{nameof(FeatureAttribute.CreateInitialStateMethodName)} is null");
		}
	}
}
