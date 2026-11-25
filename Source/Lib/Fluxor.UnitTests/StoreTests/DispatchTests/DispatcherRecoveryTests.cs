using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Fluxor.UnitTests.StoreTests.DispatchTests.SupportFiles;
using Xunit;

namespace Fluxor.UnitTests.StoreTests.DispatchTests;

public class DispatcherRecoveryTests
{
	[Fact]
	public async Task WhenReducerThrows_ThenStoreStillReducesLaterActions()
	{
		var dispatcher = new Dispatcher();
		var store = new Store(dispatcher);
		var feature = new CounterFeature();
		feature.AddReducer(new CounterReducer());
		store.AddFeature(feature);
		await store.InitializeAsync();

		Assert.Throws<InvalidOperationException>(() => dispatcher.Dispatch(new Boom()));
		dispatcher.Dispatch(new Increment());

		Assert.Equal(1, feature.State);
	}

	[Fact]
	public async Task WhenEffectDispatchesTwoActionsAndTheFirstOnesReducerThrows_ThenTheSecondIsReducedBeforeDispatchReturns()
	{
		var dispatcher = new Dispatcher();
		var store = new Store(dispatcher);
		var feature = new CounterFeature();
		feature.AddReducer(new CounterReducer());
		store.AddFeature(feature);
		store.AddEffect(new EffectThatEmitsActions(new object[] { new Boom(), new Increment() }));
		await store.InitializeAsync();

		Assert.Throws<InvalidOperationException>(() => dispatcher.Dispatch(new TestAction()));

		// No further Dispatch: the Increment queued behind the failing Boom must already be reduced
		Assert.Equal(1, feature.State);
	}

	[Fact]
	public void WhenListenerThrows_ThenActionsQueuedBehindItAreDeliveredBeforeDispatchReturns()
	{
		var a = new object();
		var b = new object();
		var exception = new InvalidOperationException();

		var received = new List<object>();
		var dispatcher = new Dispatcher();
		dispatcher.ActionDispatched += (_, args) =>
		{
			received.Add(args.Action);
			if (args.Action != a)
				return;

			dispatcher.Dispatch(b);
			throw exception;
		};

		var thrown = Assert.Throws<InvalidOperationException>(() => dispatcher.Dispatch(a));

		Assert.Same(exception, thrown);
		Assert.Equal(new[] { a, b }, received);
	}

	[Fact]
	public void WhenTwoQueuedActionsThrow_ThenBothRunAndAnAggregateExceptionHoldsBothInOrder()
	{
		var a = new object();
		var b = new object();
		var ex1 = new InvalidOperationException("first");
		var ex2 = new InvalidOperationException("second");

		var received = new List<object>();
		var dispatcher = new Dispatcher();
		dispatcher.ActionDispatched += (_, args) =>
		{
			received.Add(args.Action);
			if (args.Action == a)
			{
				dispatcher.Dispatch(b);
				throw ex1;
			}
			throw ex2;
		};

		var aggregate = Assert.Throws<AggregateException>(() => dispatcher.Dispatch(a));

		Assert.Equal(new[] { a, b }, received);
		Assert.Equal(new[] { "first", "second" }, aggregate.InnerExceptions.Select(e => e.Message));
	}

	[Fact]
	public void WhenListenerThrowsForEveryAction_ThenDrainEndsAndEveryActionIsDeliveredOnce()
	{
		var a = new object();
		var b = new object();
		var c = new object();
		var d = new object();

		var received = new List<object>();
		var dispatcher = new Dispatcher();
		dispatcher.ActionDispatched += (_, args) =>
		{
			received.Add(args.Action);
			if (args.Action == a)
			{
				dispatcher.Dispatch(b);
				dispatcher.Dispatch(c);
			}
			throw new InvalidOperationException();
		};

		var aggregate = Assert.Throws<AggregateException>(() => dispatcher.Dispatch(a));

		Assert.Equal(new[] { a, b, c }, received);
		Assert.Equal(3, aggregate.InnerExceptions.Count);

		Assert.Throws<InvalidOperationException>(() => dispatcher.Dispatch(d));
		Assert.Equal(new[] { a, b, c, d }, received);
	}

	[Fact]
	public void WhenListenerUnsubscribesAfterThrowing_ThenRemainingActionsStayQueuedUntilASubscriberArrives()
	{
		var a = new object();
		var b = new object();

		var received = new List<object>();
		var dispatcher = new Dispatcher();
		EventHandler<ActionDispatchedEventArgs> listener = null;
		listener = (_, args) =>
		{
			received.Add(args.Action);
			dispatcher.Dispatch(b);
			dispatcher.ActionDispatched -= listener;
			throw new InvalidOperationException();
		};
		dispatcher.ActionDispatched += listener;

		Assert.Throws<InvalidOperationException>(() => dispatcher.Dispatch(a));
		Assert.Equal(new[] { a }, received);

		var receivedByNewListener = new List<object>();
		dispatcher.ActionDispatched += (_, args) => receivedByNewListener.Add(args.Action);

		Assert.Equal(new[] { b }, receivedByNewListener);
	}

	[Fact]
	public async Task WhenAnotherThreadDispatchesDuringTheDrainAfterAThrow_ThenItIsDeliveredExactlyOnce()
	{
		var timeout = TimeSpan.FromSeconds(5);
		var a = new object();
		var b = new object();

		var received = new List<object>();
		using var listenerEntered = new ManualResetEventSlim();
		using var otherThreadDispatched = new ManualResetEventSlim();
		var dispatcher = new Dispatcher();
		dispatcher.ActionDispatched += (_, args) =>
		{
			lock (received)
				received.Add(args.Action);
			if (args.Action != a)
				return;

			listenerEntered.Set();
			Assert.True(otherThreadDispatched.Wait(timeout), "The other thread did not finish dispatching in time");
			throw new InvalidOperationException();
		};

		var otherThread = Task.Run(() =>
		{
			Assert.True(listenerEntered.Wait(timeout), "The listener was not entered in time");
			dispatcher.Dispatch(b);
			otherThreadDispatched.Set();
		});

		Assert.Throws<InvalidOperationException>(() => dispatcher.Dispatch(a));
		await otherThread.WaitAsync(timeout);

		lock (received)
			Assert.Equal(new[] { a, b }, received);
	}

	[Fact]
	public void WhenActionIsNull_ThenDispatchThrowsArgumentNullExceptionAndDeliversNothing()
	{
		var received = new List<object>();
		var dispatcher = new Dispatcher();
		dispatcher.ActionDispatched += (_, args) => received.Add(args.Action);

		var thrown = Assert.Throws<ArgumentNullException>(() => dispatcher.Dispatch(null));

		Assert.Equal("action", thrown.ParamName);
		Assert.Empty(received);
	}

	[Fact]
	public void WhenNullIsDispatchedWithoutSubscribers_ThenItIsRejectedImmediatelyAndNeverQueued()
	{
		var dispatcher = new Dispatcher();

		var thrown = Assert.Throws<ArgumentNullException>(() => dispatcher.Dispatch(null));
		Assert.Equal("action", thrown.ParamName);

		// A queued null would be delivered (and fail) as soon as the first subscriber arrives
		var received = new List<object>();
		dispatcher.ActionDispatched += (_, args) => received.Add(args.Action);
		Assert.Empty(received);
	}

	[Fact]
	public async Task WhenSeveralThreadsDispatchAtTheSameTime_ThenListenersNeverRunConcurrentlyAndEveryActionIsDeliveredOnce()
	{
		const int threadCount = 4;
		const int rounds = 300;
		var timeout = TimeSpan.FromSeconds(5);
		var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(20);

		for (int round = 0; round < rounds && DateTime.UtcNow < deadline; round++)
		{
			int active = 0;
			int maxActive = 0;
			int delivered = 0;
			var dispatcher = new Dispatcher();
			dispatcher.ActionDispatched += (_, _) =>
			{
				int now = Interlocked.Increment(ref active);
				int seen;
				while ((seen = Volatile.Read(ref maxActive)) < now && Interlocked.CompareExchange(ref maxActive, now, seen) != seen)
				{
				}
				Thread.SpinWait(2000);
				Interlocked.Decrement(ref active);
				Interlocked.Increment(ref delivered);
			};

			using var barrier = new Barrier(threadCount);
			var tasks = Enumerable.Range(0, threadCount)
				.Select(_ => Task.Factory.StartNew(() =>
				{
					Assert.True(barrier.SignalAndWait(timeout), "The threads did not meet in time");
					dispatcher.Dispatch(new object());
				}, TaskCreationOptions.LongRunning))
				.ToArray();
			await Task.WhenAll(tasks).WaitAsync(timeout);

			Assert.Equal(1, maxActive);
			Assert.Equal(threadCount, delivered);
		}
	}

	[Fact]
	public void WhenTheDrainThreadIsInterruptedOutsideAListener_ThenTheExceptionSurfacesAndALaterDispatchIsStillDelivered()
	{
		var timeout = TimeSpan.FromSeconds(5);
		var a = new object();
		var b = new object();

		var received = new List<object>();
		using var listenerEntered = new ManualResetEventSlim();
		using var releaseListener = new ManualResetEventSlim();
		using var listenerFinished = new ManualResetEventSlim();
		var dispatcher = new Dispatcher();
		dispatcher.ActionDispatched += (_, args) =>
		{
			lock (received)
				received.Add(args.Action);
			listenerEntered.Set();
			Assert.True(releaseListener.Wait(timeout), "The listener was not released in time");
			listenerFinished.Set();
		};

		// Hold the dispatcher's lock so the drain thread blocks on it between two listener calls,
		// which is outside the try/catch that surrounds the listener call itself
		object syncRoot = typeof(Dispatcher)
			.GetField("SyncRoot", BindingFlags.Instance | BindingFlags.NonPublic)
			.GetValue(dispatcher);

		Exception drainFailure = null;
		var drainThread = new Thread(() =>
		{
			try
			{
				dispatcher.Dispatch(a);
			}
			catch (Exception ex)
			{
				drainFailure = ex;
			}
		});
		drainThread.Start();
		try
		{
			Assert.True(listenerEntered.Wait(timeout), "The listener was not entered in time");

			Monitor.Enter(syncRoot);
			try
			{
				releaseListener.Set();
				Assert.True(listenerFinished.Wait(timeout), "The listener did not finish in time");

				// Once the listener has returned, the only thing the drain thread can wait for is our lock
				var deadline = DateTime.UtcNow + timeout;
				bool blocked;
				while (!(blocked = drainThread.ThreadState.HasFlag(ThreadState.WaitSleepJoin)) && DateTime.UtcNow < deadline)
					Thread.Yield();
				Assert.True(blocked, "The drain thread never blocked on the lock");
				drainThread.Interrupt();
			}
			finally
			{
				Monitor.Exit(syncRoot);
			}
			Assert.True(drainThread.Join(timeout), "The drain thread did not finish in time");
		}
		finally
		{
			releaseListener.Set();
		}

		Assert.IsType<ThreadInterruptedException>(drainFailure);

		dispatcher.Dispatch(b);

		lock (received)
			Assert.Equal(new[] { a, b }, received);
	}
}
