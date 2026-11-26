using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.UnitTests.StoreTests.LifecycleTests;

public class LifecycleTests
{
	private record TestAction;

	private readonly List<string> Log = new();
	private readonly Dispatcher Dispatcher = new();
	private readonly Store Subject;

	public LifecycleTests()
	{
		Subject = new Store(Dispatcher);
	}

	[Fact]
	public async Task WhenMiddlewaresAreAddedBeforeInitializing_ThenEachIsInitializedThenAllAreToldInitializationFinished()
	{
		Subject.AddMiddleware(new RecordingMiddleware("A", Log));
		Subject.AddMiddleware(new RecordingMiddleware("B", Log));

		await Subject.InitializeAsync();

		Assert.Equal(
			new[]
			{
				"A.InitializeAsync", "B.InitializeAsync",
				"A.AfterInitializeAllMiddlewares", "B.AfterInitializeAllMiddlewares",
				"A.BeforeDispatch StoreInitializedAction", "B.BeforeDispatch StoreInitializedAction",
				"A.AfterDispatch StoreInitializedAction", "B.AfterDispatch StoreInitializedAction"
			},
			Log);
	}

	[Fact]
	public async Task WhenAnActionIsDispatched_ThenMiddlewareSeesItBeforeAndAfterFeaturesAndSubscribers()
	{
		Subject.AddMiddleware(new RecordingMiddleware("M", Log));
		await Subject.InitializeAsync();
		Subject.SubscribeToAction<TestAction>(this, _ => { lock (Log) Log.Add("Subscriber"); });
		Log.Clear();

		Dispatcher.Dispatch(new TestAction());

		Assert.Equal(new[] { "M.BeforeDispatch TestAction", "Subscriber", "M.AfterDispatch TestAction" }, Log);
	}

	[Fact]
	public async Task WhenCreated_ThenStoreInitializedActionIsDispatchedOnceTheStoreIsInitialized()
	{
		int received = 0;
		Subject.SubscribeToAction<StoreInitializedAction>(this, _ => received++);
		Assert.Equal(0, received);

		await Subject.InitializeAsync();

		Assert.Equal(1, received);
	}

	[Fact]
	public async Task WhenActionsAreDispatchedBeforeInitializing_ThenTheyAreProcessedInOrderOnInitialize()
	{
		var received = new List<object>();
		Subject.SubscribeToAction<object>(this, received.Add);
		var first = new TestAction();
		var second = new TestAction();

		Dispatcher.Dispatch(first);
		Dispatcher.Dispatch(second);
		Assert.Empty(received);

		await Subject.InitializeAsync();

		Assert.Equal(3, received.Count);
		Assert.IsType<StoreInitializedAction>(received[0]);
		Assert.Same(first, received[1]);
		Assert.Same(second, received[2]);
	}

	[Fact]
	public async Task WhenInitialized_ThenTheInitializedTaskCompletes()
	{
		Assert.False(Subject.Initialized.IsCompleted);

		await Subject.InitializeAsync();

		Assert.True(Subject.Initialized.IsCompletedSuccessfully);
	}

	[Fact]
	public async Task WhenInitializeAsyncIsCalledTwice_ThenMiddlewareIsInitializedOnce()
	{
		var middleware = new RecordingMiddleware("M", Log);
		Subject.AddMiddleware(middleware);

		await Subject.InitializeAsync();
		await Subject.InitializeAsync();

		Assert.Equal(1, middleware.InitializeCount);
	}

	[Fact]
	public async Task WhenInitializeAsyncIsCalledAgainWhileStillInitializing_ThenMiddlewareIsInitializedOnce()
	{
		var middleware = new RecordingMiddleware("M", Log)
		{
			InitializeGate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously)
		};
		Subject.AddMiddleware(middleware);

		Task first = Subject.InitializeAsync();
		Task second = Subject.InitializeAsync();
		middleware.InitializeGate.SetResult(true);
		await Task.WhenAll(first, second);

		Assert.Equal(1, middleware.InitializeCount);
	}

	[Fact]
	public async Task WhenInitializingFails_ThenInitializeAsyncCanBeRetried()
	{
		var middleware = new FailsOnceMiddleware();
		Subject.AddMiddleware(middleware);

		await Assert.ThrowsAsync<InvalidOperationException>(() => Subject.InitializeAsync());
		Assert.False(Subject.Initialized.IsCompleted);

		await Subject.InitializeAsync();

		Assert.Equal(2, middleware.InitializeCount);
		Assert.True(Subject.Initialized.IsCompletedSuccessfully);
	}

	private class FailsOnceMiddleware : Middleware
	{
		public int InitializeCount { get; private set; }

		public override Task InitializeAsync(IDispatcher dispatcher, IStore store) =>
			++InitializeCount == 1
				? Task.FromException(new InvalidOperationException("First initialization fails"))
				: Task.CompletedTask;
	}

	[Fact]
	public async Task WhenDisposed_ThenDispatchedActionsAreNoLongerProcessed()
	{
		await Subject.InitializeAsync();
		int received = 0;
		Subject.SubscribeToAction<TestAction>(this, _ => received++);

		((IDisposable)Subject).Dispose();
		Dispatcher.Dispatch(new TestAction());

		Assert.Equal(0, received);
	}

	[Fact]
	public async Task WhenAnEffectThrowsSynchronously_ThenUnhandledExceptionIsRaisedAndOtherEffectsStillRun()
	{
		await Subject.InitializeAsync();
		var thrown = new InvalidOperationException("Thrown synchronously");
		var otherEffectRan = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var unhandled = new TaskCompletionSource<Exception>(TaskCreationOptions.RunContinuationsAsynchronously);
		Subject.AddEffect(new SynchronouslyThrowingEffect(thrown));
		Subject.AddEffect(new SignallingEffect(otherEffectRan));
		Subject.UnhandledException += (_, e) => unhandled.TrySetResult(e.Exception);

		Dispatcher.Dispatch(new TestAction());

		// Generous timeout: the event is raised from a thread-pool task, which can be slow on a busy CI agent
		using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
		Assert.True(await otherEffectRan.Task.WaitAsync(timeout.Token));
		Assert.Same(thrown, await unhandled.Task.WaitAsync(timeout.Token));
	}

	private class SynchronouslyThrowingEffect : IEffect
	{
		private readonly Exception Exception;
		public SynchronouslyThrowingEffect(Exception exception) => Exception = exception;
		public bool ShouldReactToAction(object action) => action is TestAction;
		public Task HandleAsync(object action, IDispatcher dispatcher) => throw Exception;
	}

	private class SignallingEffect : IEffect
	{
		private readonly TaskCompletionSource<bool> Signal;
		public SignallingEffect(TaskCompletionSource<bool> signal) => Signal = signal;
		public bool ShouldReactToAction(object action) => action is TestAction;
		public Task HandleAsync(object action, IDispatcher dispatcher)
		{
			Signal.TrySetResult(true);
			return Task.CompletedTask;
		}
	}
}
