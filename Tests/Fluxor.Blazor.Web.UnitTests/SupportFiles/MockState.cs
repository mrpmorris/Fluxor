using System;

namespace Fluxor.Blazor.Web.UnitTests.SupportFiles
{
	public class MockState<T> : IState, IState<T>
	{
		T IState<T>.Value => throw new NotImplementedException();

		public int SubscribeCount { get; private set; }
		public int UnsubscribeCount { get; private set; }
		public int GenericSubscribeCount { get; private set; }
		public int GenericUnsubscribeCount { get; private set; }

		event EventHandler IState.StateChanged
		{
			add
			{
				SubscribeCount++;
			}

			remove
			{
				UnsubscribeCount++;
			}
		}

		event EventHandler<T> IState<T>.StateChanged
		{
			add
			{
				GenericSubscribeCount++;
			}

			remove
			{
				GenericUnsubscribeCount++;
			}
		}
	}
}
