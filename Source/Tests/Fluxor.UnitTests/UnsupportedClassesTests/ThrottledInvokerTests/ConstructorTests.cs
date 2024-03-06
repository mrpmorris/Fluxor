using Fluxor.UnsupportedClasses;
using System;
using Xunit;

namespace Fluxor.UnitTests.UnsupportedClassesTests.ThrottledInvokerTests
{
	public class ConstructorTests
	{
		[Fact]
		public void WhenActionIsNull_ThenThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(
				"throttledAction",
				() => new ThrottledInvoker(null));
		}
	}
}
