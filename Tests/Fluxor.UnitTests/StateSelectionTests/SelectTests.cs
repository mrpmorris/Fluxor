using System;
using Xunit;

namespace Fluxor.UnitTests.StateSelectionTests
{
	public class SelectTests : TestsBase
	{
		[Fact]
		public void WhenSelectIsCalledTwice_ThenThrowsInvalidOperationException()
		{
			Subject.Select(x => x[0]);
			Assert.Throws<InvalidOperationException>(() => Subject.Select(x => x[1]));
		}
	}
}
