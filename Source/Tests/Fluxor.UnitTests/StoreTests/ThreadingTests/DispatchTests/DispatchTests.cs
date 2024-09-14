using Fluxor.UnitTests.StoreTests.ThreadingTests.DispatchTests.SupportFiles;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.UnitTests.StoreTests.ThreadingTests.DispatchTests;

public class DispatchTests
{
	const int NumberOfThreads = 10;
	const int NumberOfIncrementsPerThread = 1000;
	volatile int NumberOfThreadsWaitingToStart = NumberOfThreads;

	private readonly IDispatcher Dispatcher;
	private readonly IStore Subject;
	private readonly IFeature<CounterState> Feature;
	private readonly ManualResetEvent StartEvent;

	[Fact]
	public async Task WhenExecutedByMultipleThreads_ThenSynchronizesStateUpdates()
	{
		await Subject.InitializeAsync();

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
			Dispatcher.Dispatch(action);
		}
	}

	public DispatchTests()
	{
		StartEvent = new ManualResetEvent(false);
		Dispatcher = new Dispatcher();
		Subject = new Store(Dispatcher);

		Feature = new CounterFeature();
		Subject.AddFeature(Feature);

		Feature.AddReducer(new IncrementCounterReducer());
	}

}
