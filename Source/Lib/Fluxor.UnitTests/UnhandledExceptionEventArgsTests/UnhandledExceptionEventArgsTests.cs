using System;
using Xunit;

namespace Fluxor.UnitTests.UnhandledExceptionEventArgsTests;

public class UnhandledExceptionEventArgsTests
{
	[Fact]
	public void WhenConstructedWithNullException_ThenThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(
			"exception",
			() => new Fluxor.Exceptions.UnhandledExceptionEventArgs(null));
	}

	[Fact]
	public void WhenConstructed_ThenIsNotHandled()
	{
		var exception = new InvalidOperationException();
		var subject = new Fluxor.Exceptions.UnhandledExceptionEventArgs(exception);

		Assert.Same(exception, subject.Exception);
		Assert.False(subject.WasHandled);
	}

	[Fact]
	public void WhenHandledIsCalled_ThenWasHandledIsTrue()
	{
		var subject = new Fluxor.Exceptions.UnhandledExceptionEventArgs(new InvalidOperationException());

		subject.Handled();

		Assert.True(subject.WasHandled);
	}
}
