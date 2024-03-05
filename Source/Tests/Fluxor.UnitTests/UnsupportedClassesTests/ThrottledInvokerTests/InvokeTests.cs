using Fluxor.UnsupportedClasses;
using System;
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
		public void WhenInvokedInsideThrottleWindow_ThenDoesntInvoke()
		{
			Subject.ThrottleWindowMs = 2000;

			for (int i = 0; i < 42; i++)
				Subject.Invoke();

			Assert.Equal(1, InvokeCount);
		}

		[Fact]
		public async Task WhenInvokedOutsideThrottleWindow_ThenInvokesImmediately()
		{
			Subject.ThrottleWindowMs = 50;
			Subject.Invoke();
			await Task.Delay(50);
			Subject.Invoke();

			Assert.Equal(2, InvokeCount);
		}

		[Fact]
		public async Task WhenExecutedByMultipleThreads_ThenThrottlesSuccessfully()
		{
			Subject.ThrottleWindowMs = 10;

			// Allow a large window for the first invoke
			int failCount = 0;
			int successCount = 0;
			var lastInvokeTime = DateTime.UtcNow.AddDays(-1);
			int smallestFailTime = int.MaxValue;

			// Set up the action to execute
			// This ensures each call is on or outside the MS window
			ActionToExecute = () =>
			{
				long elapsedMs = (int)(DateTime.UtcNow - lastInvokeTime).TotalMilliseconds;
				if (elapsedMs >= Subject.ThrottleWindowMs)
					successCount++;
				else
				{
					Interlocked.Increment(ref failCount);
					smallestFailTime = (int)Math.Min(smallestFailTime, elapsedMs);
				}
				lastInvokeTime = DateTime.UtcNow;
			};

			var startTime = DateTime.UtcNow;
			await Parallel.ForEachAsync(Enumerable.Range(1, 256), async (x, _) =>
			{
				while (successCount < 10)
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
