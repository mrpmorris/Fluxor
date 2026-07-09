using System;

namespace Fluxor.Blazor.Web.UnitTests.SupportFiles;

public class MockState<T> : IStateChangedNotifier, IState<T>
{
	T IState<T>.Value => throw new NotImplementedException();

	public int SubscribeCount { get; private set; }
	public int UnsubscribeCount { get; private set; }

	event EventHandler IStateChangedNotifier.StateChanged
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

}
