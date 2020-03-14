using Fluxor.UnitTests.SupportFiles;
using System;
using System.Collections.Generic;
using Xunit;

namespace Fluxor.UnitTests
{
	public class MiddlewareTests
	{
		[Fact]
		public void IsInsideMiddlewareChange_ShouldBeTrue_UntilBeginInternalMiddlewareChangeIsDisposed()
		{
			var subject = new MiddlewareWithExposedMembers();
			var disposables = new Queue<IDisposable>();

			Assert.Equal(0, subject._BeginMiddlewareChangeCount);
			Assert.False(subject._IsInsideMiddlewareChange);

			int expectedChangeCount = 0;
			for (int i = 1; i <= 10; i++)
			{
				expectedChangeCount = i;
				IDisposable disposable = subject._BeginInternalMiddlewareChange();
				disposables.Enqueue(disposable);

				Assert.Equal(expectedChangeCount, subject._BeginMiddlewareChangeCount);
				Assert.True(subject._IsInsideMiddlewareChange);
			}
			do
			{
				Assert.Equal(expectedChangeCount, subject._BeginMiddlewareChangeCount);
				Assert.True(subject._IsInsideMiddlewareChange);

				disposables.Dequeue().Dispose();
				expectedChangeCount--;
			} while (disposables.Count > 0);

			Assert.Equal(0, subject._BeginMiddlewareChangeCount);
			Assert.False(subject._IsInsideMiddlewareChange);
		}

		[Fact]
		public void OnInternalMiddlewareChangeEnding_ShouldOnlyBeCalled_WhenLastInternalChangeCallHasCompleted()
		{
			var subject = new MiddlewareWithExposedMembers();
			Assert.Equal(0, subject.OnInternalMiddlewareChangeEndingCallCount);

			subject._BeginInternalMiddlewareChange().Dispose();
			Assert.Equal(1, subject.OnInternalMiddlewareChangeEndingCallCount);

			using (subject._BeginInternalMiddlewareChange())
			{
				using (subject._BeginInternalMiddlewareChange())
				{

				}
			}
			Assert.Equal(2, subject.OnInternalMiddlewareChangeEndingCallCount);
		}
	}
}
