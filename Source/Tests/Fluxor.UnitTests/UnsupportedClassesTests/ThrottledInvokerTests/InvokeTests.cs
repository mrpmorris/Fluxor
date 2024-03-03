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
			int processorCount = Environment.ProcessorCount;

			ManualResetEvent[] threadReadyEvents =
				Enumerable.Range(0, processorCount)
				.Select(x => new ManualResetEvent(false))
				.ToArray();

			ManualResetEvent[] threadCompletedEvents =
				Enumerable.Range(0, processorCount)
				.Select(x => new ManualResetEvent(false))
				.ToArray();

			bool testCompleted = false;
			var startTestEvent = new ManualResetEvent(false);
			Subject.ThrottleWindowMs = 100;

			for (int i = 0; i < processorCount; i++)
			{
				ManualResetEvent threadReadyEvent = threadReadyEvents[i];
				ManualResetEvent threadCompletedEvent = threadCompletedEvents[i];
				new Thread(_ => 
				{
					threadReadyEvent.Set();
					startTestEvent.WaitOne();
					while (!testCompleted)
						Subject.Invoke();
					threadCompletedEvent.Set();
				}).Start();
			}

			// Wait for all threads to be ready
			WaitHandle.WaitAll(threadReadyEvents);

			// Allow a large window for the first invoke
			int failCount = 0;
			var lastInvokeTime = DateTime.UtcNow.AddDays(-1);
			int smallestFailTime = int.MaxValue;

			// Set up the action to execute
			// This ensures each call is on or outside the MS window
			ActionToExecute = () =>
			{
				long elapsedMs = (int)(DateTime.UtcNow - lastInvokeTime).TotalMilliseconds;
				if (elapsedMs < Subject.ThrottleWindowMs)
				{
					Interlocked.Increment(ref failCount);
					smallestFailTime = (int)Math.Min(smallestFailTime, elapsedMs);
				}
				lastInvokeTime = DateTime.UtcNow;
			};

			// Start the test
			startTestEvent.Set();

			await Task.Delay(Subject.ThrottleWindowMs * 4);

			testCompleted = true;

			// Wait for all threads to finish
			WaitHandle.WaitAll(threadCompletedEvents);

			if (failCount > 0)
				Assert.Fail($"Failed: Smallest elapsed time was {smallestFailTime}" +
					$" when it should be {Subject.ThrottleWindowMs}" +
					$" failed {failCount} times.");
		}
	}
}
