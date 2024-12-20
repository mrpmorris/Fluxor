using System;
using System.IO;
using Xunit;

namespace Fluxor.UnitTests.DisposableCallbackTests;

public class DisposeTests
{
	[Fact]
	public void WhenCalled_ThenCallsActionPassedInConstructor()
	{
		bool wasCalled = false;
		Action action = () => wasCalled = true;
		var subject = new DisposableCallback(
			$"{nameof(DisposeTests)}.{nameof(WhenCalled_ThenCallsActionPassedInConstructor)}",
			action);

		Assert.False(wasCalled);
		subject.Dispose();
		Assert.True(wasCalled);
	}

	[Fact]
	public void WhenCalledTwice_ThenThrowsObjectDisposedExceptionWithCallerInformation()
	{
		string ExpectedSubstring = 
			@"Fluxor.UnitTests\DisposableCallbackTests\DisposeTests.cs"" on line "
			.Replace(Path.PathSeparator, '/');

	var subject = new DisposableCallback(
			$"{nameof(DisposeTests)}.{nameof(WhenCalledTwice_ThenThrowsObjectDisposedExceptionWithCallerInformation)}",
			() => { });
		subject.Dispose();
		var exception = Assert.Throws<ObjectDisposedException>(() => subject.Dispose());

		Assert.Contains(
			ExpectedSubstring,
			exception.Message.Replace(Path.PathSeparator, '/')
		);
	}
}