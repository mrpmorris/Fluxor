using Fluxor.UnitTests.StoreTests.ThreadingTests.CounterStore;
using Fluxor.UnitTests.SupportFiles;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.UnitTests.StoreTests.ThreadingTests
{
	public class Dispatch
	{
		const int NumberOfThreads = 10;
		const int NumberOfIncrementsPerThread = 1000;
		volatile int NumberOfThreadsWaitingToStart = NumberOfThreads;

		AbstractStore Store;
		IFeature<CounterState> Feature;
		ManualResetEvent StartEvent;

		[Fact]
		public async Task DoesNotLoseState()
		{
			await Store.InitializeAsync();

			var threads = new List<Thread>();
			for (int i = 0; i < NumberOfThreads; i++)
			{
				var thread = new Thread(IncrementCounterInThread);
				thread.Start();
				threads.Add(thread);
			}
			while (NumberOfThreadsWaitingToStart > 0)
				Thread.Sleep(50);

			StartEvent.Set();
			foreach (Thread thread in threads)
				thread.Join();

			Assert.Equal(NumberOfThreads * NumberOfIncrementsPerThread, Feature.State.Counter);
		}

		private void IncrementCounterInThread()
		{
			Interlocked.Decrement(ref NumberOfThreadsWaitingToStart);
			StartEvent.WaitOne();
			var action = new IncrementCounterAction();
			for (int i = 0; i < NumberOfIncrementsPerThread; i++)
			{
				Store.Dispatch(action);
			}
		}

		public Dispatch()
		{
			StartEvent = new ManualResetEvent(false);
			Store = new TestStore();

			Feature = new CounterFeature();
			Store.AddFeature(Feature);

			Feature.AddReducer(new IncrementCounterReducer());
		}

	}
}
