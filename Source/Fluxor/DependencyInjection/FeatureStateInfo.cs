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

		private Func<object> CreateFactoryFromParameterlessConstructor(
			FeatureStateAttribute featureStateAttribute)
		{
			ConstructorInfo constructor = StateType.GetConstructor(
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
				binder: null,
				CallingConventions.HasThis,
				types: Array.Empty<Type>(),
				modifiers: null);
			
			if (constructor == null)
				throw new ArgumentException(
					message: 
					$"{StateType.FullName} doesn't have a public or non-public parameterless constructor."
					+ $" Either create one, or specify a static factory method name using the"
					+ $" {nameof(FeatureStateAttribute.CreateInitialStateMethodName)} property on the "
					+ $" {nameof(FeatureStateAttribute)}");

			return () => Activator.CreateInstance(StateType, nonPublic: true);
		}

		private Func<object> CreateFactoryFromStateMethod(FeatureStateAttribute featureStateAttribute)
		{
			MethodInfo result =
				StateType.GetMethod(
					name: featureStateAttribute.CreateInitialStateMethodName,
					bindingAttr: BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

			if (result?.ReturnType != StateType)
				result = null;

			if ((result?.GetParameters()?.Length ?? 0) != 0)
				result = null;

			if (result == null)
				throw new InvalidOperationException(
					message: $"{StateType.FullName}.{featureStateAttribute.CreateInitialStateMethodName}"
					+ $" must be a parameterless method, and return type {StateType.FullName}");

			return (Func<object>)result.CreateDelegate(typeof(Func<object>));
		}
	}
}
