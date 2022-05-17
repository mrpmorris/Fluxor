using Fluxor.UnitTests.MockFactories;
using Fluxor.UnitTests.StoreTests.DispatchTests.SupportFiles;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace Fluxor.UnitTests.StoreTests.DispatchTests
{
	public class DispatchTests
	{
		private readonly IDispatcher Dispatcher;
		private readonly IStore Subject;

		[Fact]
		public void WhenActionIsNull_ThenThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => Dispatcher.Dispatch(null));
		}

		[Fact]
		public void WhenIsInsideMiddlewareChange_ThenDoesNotDispatchActions()
		{
			var mockMiddleware = MockMiddlewareFactory.Create();

			Subject.AddMiddleware(mockMiddleware.Object);

			var testAction = new TestAction();
			using (Subject.BeginInternalMiddlewareChange())
			{
				Dispatcher.Dispatch(testAction);
			}

			mockMiddleware.Verify(x => x.MayDispatchAction(testAction), Times.Never);
		}

		[Fact]
		public void WhenMiddlewareForbidsIt_ThenDoesNotSendActionToFeatures()
		{
			var testAction = new TestAction();
			var mockFeature = MockFeatureFactory.Create();
			var mockMiddleware = MockMiddlewareFactory.Create();
			mockMiddleware
				.Setup(x => x.MayDispatchAction(testAction))
				.Returns(false);

			Dispatcher.Dispatch(testAction);

			mockFeature
				.Verify(x => x.ReceiveDispatchNotificationFromStore(testAction), Times.Never);
		}

		[Fact]
		public void WhenCalled_ThenCallsBeforeDispatchOnAllMiddlewares()
		{
			var testAction = new TestAction();
			var mockMiddleware = MockMiddlewareFactory.Create();
			Subject.AddMiddleware(mockMiddleware.Object);

			Dispatcher.Dispatch(testAction);

			mockMiddleware
				.Verify(x => x.BeforeDispatch(testAction), Times.Once);
		}

		[Fact]
		public void WhenCalled_ThenPassesActionOnToAllFeatures()
		{
			var mockFeature = MockFeatureFactory.Create();
			Subject.AddFeature(mockFeature.Object);

			var testAction = new TestAction();
			Dispatcher.Dispatch(testAction);

			mockFeature
				.Verify(x => x.ReceiveDispatchNotificationFromStore(testAction));
		}

		[Fact]
		public void WhenFinished_ThenDispatchesTasksFromRegisteredEffects()
		{
			var mockFeature = MockFeatureFactory.Create();
			var actionToEmit1 = new TestActionFromEffect1();
			var actionToEmit2 = new TestActionFromEffect2();
			var actionsToEmit = new object[] { actionToEmit1, actionToEmit2 };
			Subject.AddFeature(mockFeature.Object);
			Subject.AddEffect(new EffectThatEmitsActions(actionsToEmit));

			Dispatcher.Dispatch(new TestAction());

			mockFeature
				.Verify(x => x.ReceiveDispatchNotificationFromStore(actionToEmit1), Times.Once);
			mockFeature
				.Verify(x => x.ReceiveDispatchNotificationFromStore(actionToEmit2), Times.Once);
		}

		[Fact]
		public void WhenCalled_ThenTriggersOnlyEffectsThatHandleTheDispatchedAction()
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
			Dispatcher.Dispatch(action);

			mockIncompatibleEffect.Verify(x => x.HandleAsync(action, It.IsAny<IDispatcher>()), Times.Never);
			mockCompatibleEffect.Verify(x => x.HandleAsync(action, It.IsAny<IDispatcher>()), Times.Once);
		}

		[Fact]
		public void WhenSynchronousEffectThrowsException_ThenStillExecutesSubsequentEffects()
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
			Dispatcher.Dispatch(action);

			mockSynchronousEffectThatThrows.Verify(x => x.HandleAsync(action, Dispatcher));
			mockEffectThatFollows.Verify(x => x.HandleAsync(action, Dispatcher));
		}

		[Fact]
		public void WhenNoSubscriberIsAttachedToTheDispatcher_ThenActionsAreStoredUntilOneSubscribes()
		{
			var dispatcher = new Dispatcher();
			dispatcher.Dispatch(0);
			dispatcher.Dispatch(1);
			dispatcher.Dispatch(2);

			var receivedActions = new List<int>();
			dispatcher.ActionDispatched += (_, args) => receivedActions.Add((int)args.Action);
			Assert.Equal(3, receivedActions.Count);
			Assert.Equal(0, receivedActions[0]);
			Assert.Equal(1, receivedActions[1]);
			Assert.Equal(2, receivedActions[2]);
		}

		public DispatchTests()
		{
			Dispatcher = new Dispatcher();
			Subject = new Store(Dispatcher);
			Subject.InitializeAsync().Wait();
		}
	}
}