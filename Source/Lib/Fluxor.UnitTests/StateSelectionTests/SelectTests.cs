using System;
using Xunit;

namespace Fluxor.UnitTests.StateSelectionTests;

public class SelectTests : TestsBase
{
	[Fact]
	public void WhenSelectIsCalledTwice_ThenThrowsInvalidOperationException()
	{
		Subject.Select(x => x[0]);
		var exception = Assert.Throws<InvalidOperationException>(() => Subject.Select(x => x[1]));
		Assert.Equal("Selector has already been set", exception.Message);
	}

	[Fact]
	public void WhenSelectorIsNull_ThenThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>("selector", () => Subject.Select(null));
	}

	[Fact]
	public void WhenConstructedWithNullFeature_ThenThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>("feature", () => new StateSelection<string, char?>(null));
	}
}
