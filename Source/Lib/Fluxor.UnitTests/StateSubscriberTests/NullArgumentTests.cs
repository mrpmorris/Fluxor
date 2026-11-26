using System;
using Xunit;

namespace Fluxor.UnitTests.StateSubscriberTests;

public class NullArgumentTests
{
	[Fact]
	public void WhenSubjectIsNull_ThenThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>("subject", () => StateSubscriber.Subscribe(null, _ => { }));
	}

	[Fact]
	public void WhenCallbackIsNull_ThenThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>("callback", () => StateSubscriber.Subscribe(new object(), null));
	}
}
