using Moq;
using System;
using Xunit;

namespace Fluxor.UnitTests.StateSubscriberTests
{
	public class SubscribeTests
	{
		private readonly Mock<IFeature<string>> MockFeature;

		private int CallbackInvocationCount;
		private string State = "X";
		private IStateSelection<string, char?> Selection { get; }

		[Fact]
		public void WhenSubscribedToSubject_ThenNotificationsShouldBeReceived()
		{
			int invocationCount = 0;
			_ = StateSubscriber.Subscribe(this, _ => invocationCount++);

			State = "Y";
			MockFeature.Raise(x => x.StateChanged += null, EventArgs.Empty);
			State = "Z";
			MockFeature.Raise(x => x.StateChanged += null, EventArgs.Empty);

			Assert.Equal(2, invocationCount);
			Assert.Equal(2, CallbackInvocationCount);
		}

		[Fact]
		public void WhenSubscriptionIsDisposed_ThenNotificationsShouldBeReceived()
		{
			int invocationCount = 0;
			using (StateSubscriber.Subscribe(this, _ => invocationCount++))
			{
				State = "Y";
				MockFeature.Raise(x => x.StateChanged += null, EventArgs.Empty);
			}
			State = "Z";
			MockFeature.Raise(x => x.StateChanged += null, EventArgs.Empty);

			Assert.Equal(1, invocationCount);
			Assert.Equal(1, CallbackInvocationCount);
		}

		public SubscribeTests()
		{
			MockFeature = new Mock<IFeature<string>>();
			MockFeature.SetupGet(x => x.State).Returns(() => State);
			Selection = new StateSelection<string, char?>(MockFeature.Object);
			Selection.Select(
				selector: x => x[0],
				valueEquals: null,
				selectedValueChanged: _ => CallbackInvocationCount++);
		}
	}
}
