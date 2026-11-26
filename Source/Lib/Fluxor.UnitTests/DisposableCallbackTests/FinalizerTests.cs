using System;
using System.Reflection;
using Xunit;

namespace Fluxor.UnitTests.DisposableCallbackTests;

/// <summary>
/// The finalizer is invoked directly; the real one is then suppressed, because an exception
/// thrown on the finalizer thread would crash the test host.
/// </summary>
public class FinalizerTests
{
	private static readonly MethodInfo Finalizer =
		typeof(DisposableCallback).GetMethod("Finalize", BindingFlags.Instance | BindingFlags.NonPublic);

	[Fact]
	public void WhenFinalizedWithoutBeingDisposed_ThenThrowsInvalidOperationExceptionWithIdAndHelpLink()
	{
		const string id = nameof(FinalizerTests) + "." + nameof(WhenFinalizedWithoutBeingDisposed_ThenThrowsInvalidOperationExceptionWithIdAndHelpLink);
		var subject = new DisposableCallback(id, () => { });
		try
		{
			var thrown = Assert.Throws<TargetInvocationException>(() => Finalizer.Invoke(subject, null));

			var exception = Assert.IsType<InvalidOperationException>(thrown.InnerException);
			Assert.StartsWith($"{nameof(DisposableCallback)} with Id ", exception.Message);
			Assert.Contains(id, exception.Message);
			Assert.Contains(@"DisposableCallbackTests\FinalizerTests.cs"" on line ", exception.Message.Replace('/', '\\'));
			Assert.EndsWith(
				" was not disposed. See https://github.com/mrpmorris/Fluxor/tree/master/Docs/disposable-callback-not-disposed.md"
					+ " for more details",
				exception.Message);
		}
		finally
		{
			GC.SuppressFinalize(subject);
		}
	}

	[Fact]
	public void WhenFinalizedAfterBeingDisposed_ThenDoesNotThrow()
	{
		var subject = new DisposableCallback(nameof(WhenFinalizedAfterBeingDisposed_ThenDoesNotThrow), () => { });
		subject.Dispose();

		Finalizer.Invoke(subject, null);
	}
}
