using Fluxor.Extensions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.UnitTests.EffectTests.ShouldReactToActionTests
{
	public class ExecuteLockedTests
	{
		[Fact]
		public async Task WhenExecuted_ThenShouldExecuteSequentially()
		{
			const int NumberOfConcurrentTasks = 32;
			const int NumberOfIncrementsPerTask = 10_000;
			const int TotalNumberOfExpectedIncrements = NumberOfConcurrentTasks * NumberOfIncrementsPerTask;

			var subject = new SpinLock();
			int readyCount = 0;
			int incrementedCount = 0;
			var tasks = new List<Task>();

			var startSignal = new TaskCompletionSource();
			for (int i = 0; i < NumberOfConcurrentTasks; i++)
			{
				var task = Task.Run(async () =>
				{
					Interlocked.Increment(ref readyCount);
					await startSignal.Task;
					subject.ExecuteLocked(() =>
					{
						for (int i = 0; i < NumberOfIncrementsPerTask; i++)
							incrementedCount++;
					});
				});
				tasks.Add(task);
			}

			while (readyCount < NumberOfConcurrentTasks)
				Thread.Yield();

			startSignal.SetResult();
			await Task.WhenAll(tasks);

			Assert.Equal(incrementedCount, TotalNumberOfExpectedIncrements);
		}
	}
}
