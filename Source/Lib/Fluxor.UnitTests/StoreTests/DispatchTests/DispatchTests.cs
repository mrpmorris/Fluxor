using Fluxor.UnitTests.MockFactories;
using Fluxor.UnitTests.StoreTests.DispatchTests.SupportFiles;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.UnitTests.StoreTests.DispatchTests;

public class DispatchTests : IAsyncLifetime
{
	private readonly IDispatcher Dispatcher;
	private readonly IStore Subject;

	[Fact]
	public async Task WhenActionIsNull_ThenThrowsArgumentNullException()
	{
		await Assert.ThrowsAsync<ArgumentNullException>(() => Dispatcher.DispatchAsync(null));
	}

	[Fact]
	public void WhenIsInsideMiddlewareChange_ThenDoesNotDispatchActions()
	{
		var mockMiddleware = MockMiddlewareFactory.Create();

		Subject.AddMiddleware(mockMiddleware.Object);

		var testAction = new TestAction();
		using (Subject.BeginInternalMiddlewareChange())
		{
			Task dispatchTask = Dispatcher.DispatchAsync(testAction);
			// The action is ignored by design, so its task completes immediately
			Assert.True(dispatchTask.IsCompletedSuccessfully);
		}

		mockMiddleware.Verify(x => x.MayDispatchAction(testAction), Times.Never);
	}

	[Fact]
	public async Task WhenMiddlewareForbidsIt_ThenDoesNotSendActionToFeatures()
	{
		var testAction = new TestAction();
		var mockFeature = MockFeatureFactory.Create();
		var mockMiddleware = MockMiddlewareFactory.Create();
		mockMiddleware
			.Setup(x => x.MayDispatchAction(testAction))
			.Returns(false);

		await Dispatcher.DispatchAsync(testAction);

		mockFeature
			.Verify(x => x.ReceiveDispatchNotificationFromStore(testAction), Times.Never);
	}

	[Fact]
	public async Task WhenVetoedByMiddleware_ThenTaskCompletesSuccessfully()
	{
		var testAction = new TestAction();
		var mockMiddleware = MockMiddlewareFactory.Create();
		mockMiddleware
			.Setup(x => x.MayDispatchAction(testAction))
			.Returns(false);
		Subject.AddMiddleware(mockMiddleware.Object);

		// A veto is normal control flow, not an error
		await Dispatcher.DispatchAsync(testAction);
	}

	[Fact]
	public async Task WhenCalled_ThenCallsBeforeDispatchOnAllMiddlewares()
	{
		var testAction = new TestAction();
		var mockMiddleware = MockMiddlewareFactory.Create();
		Subject.AddMiddleware(mockMiddleware.Object);

		await Dispatcher.DispatchAsync(testAction);

		mockMiddleware
			.Verify(x => x.BeforeDispatch(testAction), Times.Once);
	}

	[Fact]
	public async Task WhenCalled_ThenPassesActionOnToAllFeatures()
	{
		var mockFeature = MockFeatureFactory.Create();
		Subject.AddFeature(mockFeature.Object);

		var testAction = new TestAction();
		await Dispatcher.DispatchAsync(testAction);

		mockFeature
			.Verify(x => x.ReceiveDispatchNotificationFromStore(testAction));
	}

	[Fact]
	public async Task WhenFinished_ThenDispatchesTasksFromRegisteredEffects()
	{
		var mockFeature = MockFeatureFactory.Create();
		var actionToEmit1 = new TestActionFromEffect1();
		var actionToEmit2 = new TestActionFromEffect2();
		var actionsToEmit = new object[] { actionToEmit1, actionToEmit2 };
		Subject.AddFeature(mockFeature.Object);
		Subject.AddEffect(new EffectThatEmitsActions(actionsToEmit));

		await Dispatcher.DispatchAsync(new TestAction());

		mockFeature
			.Verify(x => x.ReceiveDispatchNotificationFromStore(actionToEmit1), Times.Once);
		mockFeature
			.Verify(x => x.ReceiveDispatchNotificationFromStore(actionToEmit2), Times.Once);
	}

	[Fact]
	public async Task WhenCalled_ThenTriggersOnlyEffectsThatHandleTheDispatchedAction()
	{
		var mockIncompatibleEffect = new Mock<IEffect>();
		mockIncompatibleEffect
			.Setup(x => x.ShouldReactToAction(It.IsAny<object>()))
			.Returns(false);
		var mockCompatibleEffect = new Mock<IEffect>();
		mockCompatibleEffect
			.Setup(x => x.ShouldReactToAction(It.IsAny<object>()))
			.Returns(true);

		Subject.AddEffect(mockIncompatibleEffect.Object);
		Subject.AddEffect(mockCompatibleEffect.Object);

		var action = new TestAction();
		await Dispatcher.DispatchAsync(action);

		mockIncompatibleEffect.Verify(x => x.HandleAsync(action, It.IsAny<IDispatcher>()), Times.Never);
		mockCompatibleEffect.Verify(x => x.HandleAsync(action, It.IsAny<IDispatcher>()), Times.Once);
	}

	[Fact]
	public async Task WhenSynchronousEffectThrowsException_ThenStillExecutesSubsequentEffects()
	{
		var action = new object();

		var mockSynchronousEffectThatThrows = new Mock<IEffect>();
		mockSynchronousEffectThatThrows
			.Setup(x => x.ShouldReactToAction(action))
			.Returns(true);
		mockSynchronousEffectThatThrows
			.Setup(x => x.HandleAsync(action, Dispatcher))
			.ThrowsAsync(new NotImplementedException());

		var mockEffectThatFollows = new Mock<IEffect>();
		mockEffectThatFollows
			.Setup(x => x.ShouldReactToAction(action))
			.Returns(true);

		Subject.AddEffect(mockSynchronousEffectThatThrows.Object);
		Subject.AddEffect(mockEffectThatFollows.Object);
		await Assert.ThrowsAsync<NotImplementedException>(() => Dispatcher.DispatchAsync(action));

		mockSynchronousEffectThatThrows.Verify(x => x.HandleAsync(action, Dispatcher));
		mockEffectThatFollows.Verify(x => x.HandleAsync(action, Dispatcher));
	}

	[Fact]
	public async Task WhenDispatched_ThenTaskDoesNotCompleteUntilAllEffectsHaveCompleted()
	{
		var effectCompletionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var mockEffect = new Mock<IEffect>();
		mockEffect
			.Setup(x => x.ShouldReactToAction(It.IsAny<object>()))
			.Returns(true);
		mockEffect
			.Setup(x => x.HandleAsync(It.IsAny<object>(), It.IsAny<IDispatcher>()))
			.Returns(effectCompletionSource.Task);
		Subject.AddEffect(mockEffect.Object);

		Task dispatchTask = Dispatcher.DispatchAsync(new TestAction());
		Assert.False(dispatchTask.IsCompleted);

		effectCompletionSource.SetResult();
		await dispatchTask.WaitAsync(TimeSpan.FromSeconds(5));
	}

	[Fact]
	public async Task WhenEffectAwaitsNestedDispatch_ThenBothTasksComplete()
	{
		Task nestedDispatchTask = null;
		var mockEffect = new Mock<IEffect>();
		mockEffect
			.Setup(x => x.ShouldReactToAction(It.IsAny<object>()))
			.Returns((object action) => action is TestAction);
		mockEffect
			.Setup(x => x.HandleAsync(It.IsAny<object>(), It.IsAny<IDispatcher>()))
			.Returns(async (object _, IDispatcher dispatcher) =>
			{
				nestedDispatchTask = dispatcher.DispatchAsync(new TestActionFromEffect1());
				await nestedDispatchTask;
			});
		Subject.AddEffect(mockEffect.Object);

		await Dispatcher.DispatchAsync(new TestAction()).WaitAsync(TimeSpan.FromSeconds(5));

		Assert.NotNull(nestedDispatchTask);
		Assert.True(nestedDispatchTask.IsCompletedSuccessfully);
	}

	[Fact]
	public void WhenNoSubscriberIsAttachedToTheDispatcher_ThenActionsAreStoredUntilOneSubscribes()
	{
		var dispatcher = new Dispatcher();
		Task task0 = dispatcher.DispatchAsync(0);
		Task task1 = dispatcher.DispatchAsync(1);
		Task task2 = dispatcher.DispatchAsync(2);

		// No store is subscribed, so the actions' tasks remain pending
		Assert.False(task0.IsCompleted);
		Assert.False(task1.IsCompleted);
		Assert.False(task2.IsCompleted);

		var receivedActions = new List<int>();
		dispatcher.ActionDispatched += (_, args) => receivedActions.Add((int)args.Action);
		Assert.Equal(3, receivedActions.Count);
		Assert.Equal(0, receivedActions[0]);
		Assert.Equal(1, receivedActions[1]);
		Assert.Equal(2, receivedActions[2]);
	}

	[Fact]
	public void WhenActionDispatchedListenerIsCalled_ThenListenerShouldBeAbleToDispatchAnAction()
	{
		var primaryAction = new object();
		var secondaryAction = new object();

		var dispatcher = new Dispatcher();
		dispatcher.ActionDispatched += (_, args) =>
		{
			if (args.Action != primaryAction)
				return;

			var thread = new Thread(_ =>
			{
				_ = dispatcher.DispatchAsync(secondaryAction);
			});
			thread.Start();
			thread.Join(millisecondsTimeout: 1_000);
			Assert.Equal(ThreadState.Stopped, thread.ThreadState);
		};
		_ = dispatcher.DispatchAsync(primaryAction);
	}

	public DispatchTests()
	{
		Dispatcher = new Dispatcher();
		Subject = new Store(Dispatcher);
	}

	async Task IAsyncLifetime.InitializeAsync() =>
		await Subject.InitializeAsync();

	Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;
}
