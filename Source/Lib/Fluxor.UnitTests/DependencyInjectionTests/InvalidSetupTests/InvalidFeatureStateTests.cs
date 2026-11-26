using Fluxor.UnitTests.DependencyInjectionTests.InvalidSetupTests.SupportFiles;
using System;
using Xunit;

namespace Fluxor.UnitTests.DependencyInjectionTests.InvalidSetupTests;

public class InvalidFeatureStateTests : InvalidSetupTestsBase
{
	[Fact]
	public void WhenStateHasNoParameterlessConstructor_ThenThrowsArgumentException()
	{
		var exception = ScanTypeAndExpect<ArgumentException>(typeof(StateWithoutParameterlessConstructor));

		Assert.StartsWith(
			$"{typeof(StateWithoutParameterlessConstructor).FullName}"
				+ " doesn't have a public or non-public parameterless constructor."
				+ " Either create one, or specify a static factory method name using the"
				+ " CreateInitialStateMethodName property on the ",
			exception.Message);
		Assert.EndsWith("FeatureStateAttribute", exception.Message);
	}

	[Theory]
	[InlineData(typeof(StateWithMissingFactoryMethod), "DoesNotExist")]
	[InlineData(typeof(StateWithFactoryMethodWithParameter), "Create")]
	[InlineData(typeof(StateWithFactoryMethodReturningWrongType), "Create")]
	[InlineData(typeof(StateWithInstanceFactoryMethod), "Create")]
	public void WhenFactoryMethodIsNotAStaticParameterlessMethodReturningTheState_ThenThrowsInvalidOperationException(
		Type stateType,
		string methodName)
	{
		var exception = ScanTypeAndExpect<InvalidOperationException>(stateType);

		Assert.Equal(
			$"{stateType.FullName}.{methodName} must be a parameterless method, and return type {stateType.FullName}",
			exception.Message);
	}
}
