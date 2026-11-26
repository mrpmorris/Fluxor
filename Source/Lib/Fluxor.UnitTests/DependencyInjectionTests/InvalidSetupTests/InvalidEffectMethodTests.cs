using Fluxor.UnitTests.DependencyInjectionTests.InvalidSetupTests.SupportFiles;
using System;
using Xunit;

namespace Fluxor.UnitTests.DependencyInjectionTests.InvalidSetupTests;

public class InvalidEffectMethodTests : InvalidSetupTestsBase
{
	[Fact]
	public void WhenActionTypeIsNotInAttributeAndMethodHasOneParameter_ThenThrowsArgumentException()
	{
		var exception = ScanTypeAndExpect<ArgumentException>(typeof(EffectWithoutDispatcherParameter));

		Assert.StartsWith(
			"Method must have 2 parameters (action, IDispatcher)"
				+ " when [EffectMethodAttribute] has no ActionType specified. "
				+ MethodDescription(typeof(EffectWithoutDispatcherParameter), "Handle"),
			exception.Message);
	}

	[Fact]
	public void WhenActionTypeIsInAttributeAndMethodAlsoHasActionParameter_ThenThrowsArgumentException()
	{
		var exception = ScanTypeAndExpect<ArgumentException>(typeof(EffectWithActionTypeAndActionParameter));

		Assert.StartsWith(
			"Method must have 1 parameter (IDispatcher)"
				+ " when [EffectMethodAttribute] has an ActionType specified. "
				+ MethodDescription(typeof(EffectWithActionTypeAndActionParameter), "Handle"),
			exception.Message);
	}

	[Fact]
	public void WhenLastParameterIsNotADispatcher_ThenThrowsArgumentException()
	{
		var exception = ScanTypeAndExpect<ArgumentException>(typeof(EffectWithoutDispatcherAsLastParameter));

		Assert.StartsWith(
			"The last parameter of a method should be an IDispatcher"
				+ " when decorated with an [EffectMethodAttribute]. "
				+ MethodDescription(typeof(EffectWithoutDispatcherAsLastParameter), "Handle"),
			exception.Message);
	}

	[Fact]
	public void WhenReturnTypeIsNotTask_ThenThrowsArgumentException()
	{
		var exception = ScanTypeAndExpect<ArgumentException>(typeof(EffectReturningVoid));

		Assert.StartsWith(
			"Effect methods must have a return type of Task. "
				+ MethodDescription(typeof(EffectReturningVoid), "Handle"),
			exception.Message);
	}
}
