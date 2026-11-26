using Fluxor.UnitTests.FeatureTests.SupportFiles;
using System;
using Xunit;

namespace Fluxor.UnitTests.FeatureTests;

public class NullArgumentTests
{
	private readonly SomeFeature Subject = new();

	[Fact]
	public void WhenAddReducerIsGivenNull_ThenThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>("reducer", () => Subject.AddReducer(null));
	}

	[Fact]
	public void WhenReceiveDispatchNotificationFromStoreIsGivenNull_ThenThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>("action", () => Subject.ReceiveDispatchNotificationFromStore(null));
	}
}
