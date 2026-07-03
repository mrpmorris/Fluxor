using Fluxor.UnitTests.MockFactories;
using Fluxor.UnitTests.StoreTests.DispatchAsyncExceptionTests.SupportFiles;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.UnitTests.StoreTests.DispatchAsyncExceptionTests;

public class DispatchAsyncExceptionTests : IAsyncLifetime
{
	private readonly IDispatcher Dispatcher;
	private readonly IStore Subject;

	[Fact]
	public async Task WhenEffectThrows_ThenDispatchAsyncTaskFaults()
	{
		await Assert.ThrowsAsync<InvalidOperationException>(
			() => Dispatcher.DispatchAsync(new ThrowSimpleExceptionAction()));
	}

	[Fact]
	public async Task WhenEffectThrowsAggregateException_ThenTaskCarriesAllExceptions()
	{
		Task dispatchTask = Dispatcher.DispatchAsync(new ThrowAggregateExceptionAction());
		await Assert.ThrowsAnyAsync<Exception>(() => dispatchTask);

		Type[] exceptionTypes = dispatchTask.Exception
			.Flatten()
			.InnerExceptions
			.Select(x => x.GetType())
			.ToArray();

		Assert.Equal(3, exceptionTypes.Length);
		Assert.Contains(typeof(InvalidOperationException), exceptionTypes);
		Assert.Contains(typeof(InvalidCastException), exceptionTypes);
		Assert.Contains(typeof(InvalidProgramException), exceptionTypes);
	}

	[Fact]
	public async Task WhenReducerThrows_ThenTaskFaultsAndSubsequentDispatchesStillWork()
	{
		var throwingAction = new ThrowSimpleExceptionAction();
		var followUpAction = new object();
		var mockFeature = MockFeatureFactory.Create();
		mockFeature
			.Setup(x => x.ReceiveDispatchNotificationFromStore(throwingAction))
			.Throws(new NotSupportedException());
		Subject.AddFeature(mockFeature.Object);

		await Assert.ThrowsAsync<NotSupportedException>(
			() => Dispatcher.DispatchAsync(throwingAction));

		// The store must still process subsequent actions
		await Dispatcher.DispatchAsync(followUpAction).WaitAsync(TimeSpan.FromSeconds(5));
		mockFeature.Verify(x => x.ReceiveDispatchNotificationFromStore(followUpAction), Times.Once);
	}

	[Fact]
	public async Task WhenReducerThrows_ThenEffectsAreNotTriggered()
	{
		var throwingAction = new ThrowSimpleExceptionAction();
		var mockFeature = MockFeatureFactory.Create();
		mockFeature
			.Setup(x => x.ReceiveDispatchNotificationFromStore(throwingAction))
			.Throws(new NotSupportedException());
		Subject.AddFeature(mockFeature.Object);

		// The reducer exception faults the task; EffectThatThrowsSimpleException must not run,
		// otherwise the task would carry its InvalidOperationException too
		Task dispatchTask = Dispatcher.DispatchAsync(throwingAction);
		await Assert.ThrowsAsync<NotSupportedException>(() => dispatchTask);
		Assert.Single(dispatchTask.Exception.Flatten().InnerExceptions);
	}

	public DispatchAsyncExceptionTests()
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
