using System;
using Xunit;

namespace Fluxor.UnitTests
{
	public class DisposableCallbackTests
	{
		public class Dispose
		{
			[Fact]
			public void CallsActionPassedInConstructor()
			{
				bool wasCalled = false;
				Action action = () => wasCalled = true;
				var subject = new DisposableCallback(action);

				Assert.False(wasCalled);
				subject.Dispose();
				Assert.True(wasCalled);
			}

			[Fact]
			public void ThrowsObjectDisposedException_WhenDisposedTwice()
			{
				var subject = new DisposableCallback(() => { });
				subject.Dispose();
				Assert.Throws<ObjectDisposedException>(() => subject.Dispose());
			}
		}
	}
}
