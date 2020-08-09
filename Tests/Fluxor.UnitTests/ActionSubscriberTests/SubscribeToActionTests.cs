using System;
using Xunit;

namespace Fluxor.UnitTests.ActionSubscriberTests
{
	public class SubscribeToActionTests
	{
		private Store Subject = new Store();

		[Fact]
		public void WhenSubscriberIsNull_ThenThrowArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>("subscriber", () => Subject.SubscribeToAction<object>(subscriber: null, callback: null));
		}

		[Fact]
		public void WhenCallbackIsNull_ThenThrowArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>("callback", () => Subject.SubscribeToAction<object>(subscriber: this, callback: null));
		}
	}
}
