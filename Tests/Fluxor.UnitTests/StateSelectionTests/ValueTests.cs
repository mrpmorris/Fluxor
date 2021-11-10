using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Fluxor.UnitTests.StateSelectionTests
{
	public class ValueTests : TestsBase
	{
		[Fact]
		public void WhenReading_AndSelectHasBeenCalled_ThrowsInvalidOperationException()
		{
			var exception = Assert.Throws<InvalidOperationException>(() => Subject.Value);
			Assert.Equal("Must call Select before accessing Value", exception.Message);
		}

		[Fact]
		public void WhenReading_AndSelectHasBeenCalled_ThenReturnsTransformedValue()
		{
			Subject.Select(x => x[2]);
			Assert.Equal('C', Subject.Value);
		}
	}
}
