using Fluxor.UnitTests.StoreTests.UnhandledExceptionTests.SupportFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;


namespace Fluxor.UnitTests.StoreTests.UnhandledExceptionTests;

public class UnhandledExceptionTests : IAsyncLifetime
{
	private readonly IDispatcher Dispatcher;
	private readonly IStore Subject;
	private volatile int NumberOfOutstandingInvokes;

	[Fact]
	public async Task WhenTriggerThrowsUnhandledException_ThenEventIsTriggered()
	{
		IEnumerable<Exception> exceptions = await SendAction(new ThrowSimpleExceptionAction(), expectedExceptionCount: 1);
		Assert.Single(exceptions);
		Assert.IsType<InvalidOperationException>(exceptions.First());
	}

	[Fact]
	public async Task WhenTriggerThrowsUnhandledAggregateException_ThenEventIsTriggeredForEachEvent()
	{
		Type[] exceptionTypes = 
			(await SendAction(new ThrowAggregateExceptionAction(), expectedExceptionCount: 3))
			.Select(x => x.GetType())
			.ToArray();

		Assert.Equal(3, exceptionTypes.Length);
		Assert.Contains(typeof(InvalidOperationException), exceptionTypes);
		Assert.Contains(typeof(InvalidCastException), exceptionTypes);
		Assert.Contains(typeof(InvalidProgramException), exceptionTypes);
	}

	private async Task<IEnumerable<Exception>> SendAction(object action, int expectedExceptionCount)
	{
		NumberOfOutstandingInvokes = expectedExceptionCount;

		var result = new List<Exception>();
		var resetEvent = new ManualResetEvent(false);

		Subject.UnhandledException += (sender, e) =>
		{
			result.Add(e.Exception);
			if (Interlocked.Decrement(ref NumberOfOutstandingInvokes) == 0)
				resetEvent.Set();
		};

		Task effectTask = Task.Run(() =>
		{
			Dispatcher.Dispatch(action);
			// Wait for Effect to say it is ready, 1 second timeout
			resetEvent.WaitOne(1000);
		});

		await effectTask.ConfigureAwait(false);
		return result;
	}

	public UnhandledExceptionTests()
	{
		Dispatcher = new Dispatcher();
		Subject = new Store(Dispatcher);
		Subject.AddEffect(new EffectThatThrowsSimpleException());
		Subject.AddEffect(new EffectThatThrowsAggregateException());
	}

	async Task IAsyncLifetime.InitializeAsync() =>
		await Subject.InitializeAsync();

	Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;
}
