using System;
using Xunit;

namespace Fluxor.UnitTests.StateSelectionTests;

public class ValueTests : TestsBase
{
	[Fact]
	public void WhenReading_AndSelectHasNotBeenCalled_ThrowsInvalidOperationException()
	{
		var exception = Assert.Throws<InvalidOperationException>(() => Subject.Value);
		Assert.Equal("Must call Select before accessing Value", exception.Message);
	}

	[Fact]
	public void WhenReading_AndSelectHasBeenCalled_ThenReturnsTransformedValue()
	{
		FeatureState = "ABC";
		Subject.Select(x => x[2]);
		Assert.Equal('C', Subject.Value);
	}
}
