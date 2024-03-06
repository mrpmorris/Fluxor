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
			Subject.ThrottleWindowMs = 50;
			var stopwatch = Stopwatch.StartNew();
			Subject.Invoke();
			do
			{
				int elapsed = (int)stopwatch.ElapsedMilliseconds;
				if (elapsed >= Subject.ThrottleWindowMs)
					break;
				await Task.Delay(Subject.ThrottleWindowMs - elapsed);
			} while (true);
			Subject.Invoke();

			Assert.Equal(2, InvokeCount);
		}

		[Fact]
		public async Task WhenExecutedByMultipleThreads_ThenThrottlesSuccessfully()
		{
			Subject.ThrottleWindowMs = 10;
			int longestAllowedDelay = Subject.ThrottleWindowMs * 2;

			// Allow a large window for the first invoke
			int tooSoonCount = 0;
			int tooLateCount = 0;
			int successCount = 0;
			int totalExecutionCount = 0;
			var lastInvokeTime = DateTime.UtcNow.AddDays(-1);

			// Set up the action to execute
			// This ensures each call is on or outside the MS window
			ActionToExecute = () =>
			{
				double elapsedMs = (DateTime.UtcNow - lastInvokeTime).TotalMilliseconds;
				Interlocked.Increment(ref totalExecutionCount);
				if (elapsedMs > Subject.ThrottleWindowMs)
					Interlocked.Increment(ref successCount);
				else
				if (elapsedMs > tooLateCount)
					Interlocked.Increment(ref tooLateCount);
				else
					Interlocked.Increment(ref tooSoonCount);
				lastInvokeTime = DateTime.UtcNow;
			};

			var startTime = DateTime.UtcNow;
			await Parallel.ForEachAsync(Enumerable.Range(1, 256), async (x, _) =>
			{
				while (totalExecutionCount < 10)
				{
					await Task.Yield();
					Subject.Invoke();
				}
			});

			if (tooSoonCount > 0)
				Assert.Fail($"Executed too soon {tooSoonCount} times.");
			if (tooLateCount > 0)
				Assert.Fail($"Executed too late {tooLateCount} times.");
		}
	}
}
