using Fluxor.UnitTests.MiddlewareTests.OnInternalMiddlewareChangeEndingTests.SupportFiles;
using Xunit;

namespace Fluxor.UnitTests.MiddlewareTests.OnInternalMiddlewareChangeEndingTests;

public class OnInternalMiddlewareChangeEndingTests
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
