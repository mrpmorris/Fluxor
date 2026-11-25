using System;
using System.Collections.Generic;
using System.Linq;
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
}
