﻿using Fluxor.UnitTests.SupportFiles;
using Xunit;

namespace Fluxor.UnitTests.MiddlewareTests
{
	public class OnInternalMiddlewareChangeEnding
	{
		[Fact]
		public void WhenLastInternalChangeCallHasCompleted_ThenShouldCallOnInternalMiddlewareChangeEnding()
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
