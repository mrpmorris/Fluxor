using Fluxor.UnsupportedClasses;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.UnitTests.UnsupportedClassesTests.ThrottledInvokerTests
{
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
			for (int i = 0; i < 42; i++)
				Subject.Invoke();

			Assert.Equal(42, InvokeCount);
		}

		[Fact]
		public void WhenInvokedInsideThrottleWindow_ThenDoesNotInvoke()
		{
			Subject.ThrottleWindowMs = 2000;

			for (int i = 0; i < 42; i++)
				Subject.Invoke();

			Assert.Equal(1, InvokeCount);
		}

		[Fact]
		public async Task WhenInvokedOutsideThrottleWindow_ThenInvokesImmediately()
		{
			const int Iterations = 10;
			Subject.ThrottleWindowMs = 50;
			var stopwatch = Stopwatch.StartNew();

			// Get the initial invoke out of the way, because this always
			// executes immediately.
			Subject.Invoke();

			// Now do X iterations
			for (int i = 0; i < Iterations; i++)
			{
				// Invoke immediately, this should not execute the callback because it is executed
				// either immediately after the initial Subject.Invoke() in the test,
				// or because it is executed immediately after the Subject.Invoke() at the end
				// of the do/while loop.
				Subject.Invoke();

				stopwatch.Restart();
				do
				{
					int elapsed = (int)stopwatch.ElapsedMilliseconds;
					if (elapsed > Subject.ThrottleWindowMs)
						break;
					await Task.Delay(Subject.ThrottleWindowMs - elapsed);
				} while (true);

				// Alone with the first one in the test (outside the loop), this is the
				// Subject.Invoke() that should be outside the throttle window and therefore
				// be executed.
				Subject.Invoke();
			}

			// This will always be +1 because we have `Iterations` executions,
			// and the one outside the loop.
			// This is to check the first call always goes straight through,
			// and all calls after that are throttled.
			Assert.Equal(Iterations + 1, InvokeCount);
		}

		[Fact]
		public async Task WhenExecutedByMultipleThreads_ThenThrottlesSuccessfully()
		{
			Subject.ThrottleWindowMs = 25;

			// Allow a large window for the first invoke
			int failCount = 0;
			int executionCount = 0;
			var lastInvokeTime = DateTime.UtcNow.AddDays(-1);
			int smallestFailTime = int.MaxValue;

			// Set up the action to execute
			// This ensures each call is on or outside the MS window
			ActionToExecute = () =>
			{
				Interlocked.Increment(ref executionCount);

				double elapsedMs = (DateTime.UtcNow - lastInvokeTime).TotalMilliseconds;
				if (elapsedMs <= Subject.ThrottleWindowMs)
				{
					Interlocked.Increment(ref failCount);
					smallestFailTime = (int)Math.Min(smallestFailTime, elapsedMs);
				}
				lastInvokeTime = DateTime.UtcNow;
			};

			var startTime = DateTime.UtcNow;
			await Parallel.ForEachAsync(Enumerable.Range(1, 256), async (x, _) =>
			{
				while (executionCount < 10)
				{
					await Task.Yield();
					Subject.Invoke();
				}
			});


			if (failCount > 0)
				Assert.Fail(
					$"Failed: Smallest elapsed time was {smallestFailTime}" +
					$" when it should be {Subject.ThrottleWindowMs}" +
					$" failed {failCount} times.");
		}
	}
}
