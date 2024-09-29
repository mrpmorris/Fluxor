using Fluxor.UnsupportedClasses;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.UnitTests.UnsupportedClassesTests.ThrottledInvokerTests;

public class InvokeTests
{
	private ThrottledInvoker Subject;
	private int InvokeCount;
	private Action ActionToExecute;

	public InvokeTests()
	{
		ActionToExecute = () => Interlocked.Increment(ref InvokeCount);
		Subject = new ThrottledInvoker(() => ActionToExecute());
	}

	[Fact]
	public void WhenNotThrottled_ThenInvokesImmediately()
	{
		for (int i = 1; i <= 42; i++)
			Subject.Invoke(maximumInvokesPerSecond: 0);

		Assert.Equal(42, InvokeCount);
	}

	[Fact]
	public void WhenInvokedInsideThrottleWindow_ThenDoesNotInvoke()
	{
		for (int i = 1; i <= 42; i++)
			Subject.Invoke(maximumInvokesPerSecond: 1);

		Assert.Equal(1, InvokeCount);
	}

	[Fact]
	public async Task WhenInvokedInsideThrottleWindow_ThenSecondInvokeIsDeferredUntilEndOfThrottleWindow()
	{
		const byte AllowedInvokesPerSecond = 10;
		const int ThrottleWindowMS = (1000 / AllowedInvokesPerSecond);
		const int HalfOfAThrottleWindowMS = ThrottleWindowMS / 2;
		const int ThreeThrottleWindowsMS = (int)(ThrottleWindowMS * 3);

		// First invoke should execute immediately.
		Subject.Invoke(maximumInvokesPerSecond: AllowedInvokesPerSecond);
		if (InvokeCount != 1)
			Assert.Fail($"Expected first Invoke in new throttle window to execute immediately. Expected 1 Invoke but found {InvokeCount}.");

		// Invoke again within the throttle window, this should defer the 2nd call
		// until the start of the next throttle window.
		await Task.Delay(HalfOfAThrottleWindowMS);
		Subject.Invoke(maximumInvokesPerSecond: AllowedInvokesPerSecond);
		if (InvokeCount != 1)
			Assert.Fail($"Expected second Invoke within throttle window get deferred until the start of the next window. Expected 1 Invoke but found {InvokeCount}.");

		// Ensure the test has finished
		await Task.Delay(ThreeThrottleWindowsMS);

		if (InvokeCount != 2)
			Assert.Fail($"Expected deferred second Invoke to have executed after the next window has started. Expected 2 Invoke but found {InvokeCount}.");
	}

	[Fact]
	public async Task WhenInvokedInsideThrottleWindow_ThenMoreThanTwoInvokesAreDiscarded()
	{
		const byte AllowedInvokesPerSecond = 1;
		const int ThrottleWindowMS = (1000 / AllowedInvokesPerSecond);
		const int ThreeThrottleWindowsMS = (int)(ThrottleWindowMS * 3);

		for (int i = 1; i <= 10; i++)
			Subject.Invoke(maximumInvokesPerSecond: AllowedInvokesPerSecond);
		if (InvokeCount != 1)
			Assert.Fail($"Expected first Invoke in new throttle window to execute immediately. Expected 1 Invoke but found {InvokeCount}.");

		await Task.Delay(ThreeThrottleWindowsMS);

		if (InvokeCount != 2)
			Assert.Fail("Deferred Invoke was not executed at start of second window.");

		if (InvokeCount > 2)
			Assert.Fail("Expected additional Invokes to be discarded when a deferred Invoke is waiting to execute.");
	}

	[Fact]
	public async Task WhenExecutedByMultipleThreads_ThenOnlyOneThreadAtATimeInvokesTheAction()
	{
		const byte AllowedInvokesPerSecond = 50;
		int concurrentExecutionCount = 0;
		int executionCount = 0;

		// Set up the action to execute
		// This ensures each call is on or outside the MS window
		ActionToExecute = () =>
		{
			Interlocked.Increment(ref executionCount);

			int newConcurrentExecutionCount = Interlocked.Increment(ref concurrentExecutionCount);
			if (newConcurrentExecutionCount != 1)
				Assert.Fail($"Concurrent execution detected (Expected 1, found {newConcurrentExecutionCount}).");
			Interlocked.Decrement(ref concurrentExecutionCount);
		};

		await Parallel.ForEachAsync(Enumerable.Range(1, 256), async (x, _) =>
		{
			while (executionCount < 10)
			{
				await Task.Yield();
				Subject.Invoke(maximumInvokesPerSecond: AllowedInvokesPerSecond);
			}
		});
	}

	[Fact]
	public async Task WhenExecutedByMultipleThreads_ThenThrottlesSuccessfully()
	{
		const byte AllowedInvokesPerSecond = 50;
		const int WindowSizeMS = (1000 / AllowedInvokesPerSecond);

		// Allow a large window for the first invoke
		long lastInvokedUtcTicks = DateTime.UtcNow.AddMinutes(-1).Ticks;

		int failCount = 0;
		int totalExecutionCount = 0;
		int smallestFailMS = int.MaxValue;

		Lock syncRoot = new();

		// Set up the action to execute
		// This ensures each call is on or outside the MS window
		ActionToExecute = () =>
		{
			long nowUtcTicks = DateTime.UtcNow.Ticks;
			int elapsedMS = (int)(Math.Ceiling(TimeSpan.FromTicks(nowUtcTicks - lastInvokedUtcTicks).TotalMilliseconds));

			lock (syncRoot)
			{
				lastInvokedUtcTicks = nowUtcTicks;

				if (elapsedMS < WindowSizeMS)
				{
					failCount++;
					if (elapsedMS < smallestFailMS)
						smallestFailMS = elapsedMS;
				}

				totalExecutionCount++;
			}
		};

		await Parallel.ForEachAsync(Enumerable.Range(1, 256), async (x, _) =>
		{
			while (totalExecutionCount < 10)
			{
				await Task.Yield();
				Subject.Invoke(maximumInvokesPerSecond: AllowedInvokesPerSecond);
			}
		});


		if (failCount > 0)
			Assert.Fail(
				$"Failed {failCount} times." +
				$" Smallest elapsed time was {TimeSpan.FromTicks(smallestFailMS).TotalMilliseconds} MS" +
				$" when it should be no less than {WindowSizeMS} MS" +
				$" failed {failCount} times.");
	}

	[Fact]
	public void WhenDisposed_ThenInitialActionDoesNotExecute()
	{
		const byte AllowedInvokesPerSecond = 10;

		(Subject as IDisposable).Dispose();
		Subject.Invoke(maximumInvokesPerSecond: AllowedInvokesPerSecond);

		Assert.Equal(0, InvokeCount);
	}

	[Fact]
	public async Task WhenDisposed_ThenAlreadyDeferredActionDoesNotExecute()
	{
		const byte AllowedInvokesPerSecond = 10;
		const int WindowSizeMS = (1000 / AllowedInvokesPerSecond);

		Subject.Invoke(maximumInvokesPerSecond: AllowedInvokesPerSecond);
		Subject.Invoke(maximumInvokesPerSecond: AllowedInvokesPerSecond);

		Subject.Dispose();

		await Task.Delay(WindowSizeMS * 2);

		Assert.Equal(1, InvokeCount);
	}
}
