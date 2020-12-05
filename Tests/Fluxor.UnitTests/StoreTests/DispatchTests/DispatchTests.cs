using Fluxor.UnitTests.MockFactories;
using Fluxor.UnitTests.StoreTests.DispatchTests.SupportFiles;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.UnitTests.StoreTests.DispatchTests
{
	public class DispatchTests
	{
		[Fact]
		public void WhenActionIsNull_ThenThrowsArgumentNullException()
		{
			var dispatcher = new Dispatcher();
			var subject = new Store(dispatcher);
			Assert.Throws<ArgumentNullException>(() => dispatcher.Dispatch(null));
		}

		[Fact]
		public async Task WhenIsInsideMiddlewareChange_ThenDoesNotDispatchActions()
		{
			var mockMiddleware = MockMiddlewareFactory.Create();

			var dispatcher = new Dispatcher();
			var subject = new Store(dispatcher);
			await subject.InitializeAsync().ConfigureAwait(false);
			subject.AddMiddleware(mockMiddleware.Object);

			var testAction = new TestAction();
			using (subject.BeginInternalMiddlewareChange())
			{
				dispatcher.Dispatch(testAction);
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

			var dispatcher = new Dispatcher();
			var subject = new Store(dispatcher);
			await subject.InitializeAsync().ConfigureAwait(false);

			dispatcher.Dispatch(testAction);

			mockFeature
				.Verify(x => x.ReceiveDispatchNotificationFromStore(testAction), Times.Never);
		}

		[Fact]
		public async Task WhenCalled_ThenCallsBeforeDispatchOnAllMiddlewares()
		{
			var testAction = new TestAction();
			var mockMiddleware = MockMiddlewareFactory.Create();

			var dispatcher = new Dispatcher();
			var subject = new Store(dispatcher);
			await subject.InitializeAsync().ConfigureAwait(false);
			subject.AddMiddleware(mockMiddleware.Object);

			dispatcher.Dispatch(testAction);

			mockMiddleware
				.Verify(x => x.BeforeDispatch(testAction), Times.Once);
		}

		[Fact]
		public async Task WhenCalled_ThenPassesActionOnToAllFeatures()
		{
			var mockFeature = MockFeatureFactory.Create();

			var dispatcher = new Dispatcher();
			var subject = new Store(dispatcher);
			subject.AddFeature(mockFeature.Object);
			await subject.InitializeAsync().ConfigureAwait(false);

			var testAction = new TestAction();
			dispatcher.Dispatch(testAction);

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

			var dispatcher = new Dispatcher();
			var subject = new Store(dispatcher);
			await subject.InitializeAsync().ConfigureAwait(false);
			subject.AddFeature(mockFeature.Object);
			subject.AddEffect(new EffectThatEmitsActions(actionsToEmit));

			dispatcher.Dispatch(new TestAction());

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

			var dispatcher = new Dispatcher();
			var subject = new Store(dispatcher);
			await subject.InitializeAsync().ConfigureAwait(false);
			subject.AddEffect(mockIncompatibleEffect.Object);
			subject.AddEffect(mockCompatibleEffect.Object);

			var action = new TestAction();
			dispatcher.Dispatch(action);

			mockIncompatibleEffect.Verify(x => x.HandleAsync(action, It.IsAny<IDispatcher>()), Times.Never);
			mockCompatibleEffect.Verify(x => x.HandleAsync(action, It.IsAny<IDispatcher>()), Times.Once);
		}

		[Fact]
		public async Task WhenSynchronousEffectThrowsException_ThenStillExecutesSubsequentEffects()
		{
			var dispatcher = new Dispatcher();
			var subject = new Store(dispatcher);
			var action = new object();

			var mockSynchronousEffectThatThrows = new Mock<IEffect>();
			mockSynchronousEffectThatThrows
				.Setup(x => x.ShouldReactToAction(action))
				.Returns(true);
			mockSynchronousEffectThatThrows
				.Setup(x => x.HandleAsync(action, dispatcher))
				.ThrowsAsync(new NotImplementedException());

			var mockEffectThatFollows = new Mock<IEffect>();
			mockEffectThatFollows
				.Setup(x => x.ShouldReactToAction(action))
				.Returns(true);

			await subject.InitializeAsync().ConfigureAwait(false);
			subject.AddEffect(mockSynchronousEffectThatThrows.Object);
			subject.AddEffect(mockEffectThatFollows.Object);
			dispatcher.Dispatch(action);

			mockSynchronousEffectThatThrows.Verify(x => x.HandleAsync(action, dispatcher));
			mockEffectThatFollows.Verify(x => x.HandleAsync(action, dispatcher));
		}
	}
}