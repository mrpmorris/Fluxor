using System;
using Xunit;

namespace Fluxor.UnitTests.StoreTests.NullArgumentTests;

public class NullArgumentTests
{
	private readonly Store Subject = new(new Dispatcher());

	[Fact]
	public void WhenAddFeatureIsGivenNull_ThenThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>("feature", () => Subject.AddFeature(null));
	}

	[Fact]
	public void WhenAddEffectIsGivenNull_ThenThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>("effect", () => Subject.AddEffect(null));
	}
}
