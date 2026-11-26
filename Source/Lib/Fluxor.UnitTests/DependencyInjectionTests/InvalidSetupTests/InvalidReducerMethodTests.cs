using Fluxor.UnitTests.DependencyInjectionTests.InvalidSetupTests.SupportFiles;
using System;
using Xunit;

namespace Fluxor.UnitTests.DependencyInjectionTests.InvalidSetupTests;

public class InvalidReducerMethodTests : InvalidSetupTestsBase
{
	[Fact]
	public void WhenActionTypeIsNotInAttributeAndMethodHasNoActionParameter_ThenThrowsArgumentException()
	{
		var exception = ScanTypeAndExpect<ArgumentException>(typeof(ReducerWithoutActionParameter));

		Assert.StartsWith(
			"Method must have 2 parameters (state, action)"
				+ " when [ReducerMethodAttribute] has no ActionType specified. "
				+ MethodDescription(typeof(ReducerWithoutActionParameter), "Reduce"),
			exception.Message);
	}

	[Fact]
	public void WhenActionTypeIsInAttributeAndMethodAlsoHasActionParameter_ThenThrowsArgumentException()
	{
		var exception = ScanTypeAndExpect<ArgumentException>(typeof(ReducerWithActionTypeAndActionParameter));

		Assert.StartsWith(
			"Method must have 1 parameter (state)"
				+ " when [ReducerMethodAttribute] has an ActionType specified. "
				+ MethodDescription(typeof(ReducerWithActionTypeAndActionParameter), "Reduce"),
			exception.Message);
	}

	[Fact]
	public void WhenReturnTypeIsNotTheStateType_ThenThrowsArgumentExceptionNamingTheStateType()
	{
		var exception = ScanTypeAndExpect<ArgumentException>(typeof(ReducerReturningWrongType));

		Assert.StartsWith(
			$"Expected reducer method to return type {typeof(TestState).FullName}. "
				+ MethodDescription(typeof(ReducerReturningWrongType), "Reduce"),
			exception.Message);
	}
}
