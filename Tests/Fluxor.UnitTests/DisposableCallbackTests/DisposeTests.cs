using System;
using Xunit;

namespace Fluxor.UnitTests.DisposableCallbackTests
{
	public class DisposeTests
	{
		[Fact]
		public void WhenCalled_ThenCallsActionPassedInConstructor()
		{
			bool wasCalled = false;
			Action action = () => wasCalled = true;
			var subject = new DisposableCallback("Test", action);

			Assert.False(wasCalled);
			subject.Dispose();
			Assert.True(wasCalled);
		}

		[Fact]
		public void WhenCalledTwice_ThenThrowsObjectDisposedException()
		{
			var subject = new DisposableCallback("Test", () => { });
			subject.Dispose();
			Assert.Throws<ObjectDisposedException>(() => subject.Dispose());
		}
	}
}