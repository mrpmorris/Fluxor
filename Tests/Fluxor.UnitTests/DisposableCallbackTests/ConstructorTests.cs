using System;
using Xunit;

namespace Fluxor.UnitTests.DisposableCallbackTests
{
	public class ConstructorTests
	{
		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("           ")]
		public void WhenIdIsNullOrWhitespace_ThenThrowsArgumentNullException(string id)
		{
			Assert.Throws<ArgumentNullException>(() => new DisposableCallback(id, () => { }));
		}

		[Fact]
		public void WhenActionIsNull_ThenThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => new DisposableCallback(
				$"{nameof(ConstructorTests)}.{nameof(WhenActionIsNull_ThenThrowsArgumentNullException)}",
				action: null));
		}
	}
}
