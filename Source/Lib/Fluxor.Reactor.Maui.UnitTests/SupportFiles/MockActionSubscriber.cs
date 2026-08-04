using System;

namespace Fluxor.Reactor.Maui.UnitTests.SupportFiles;

public class MockActionSubscriber : IActionSubscriber
{
	public IDisposable GetActionUnsubscriberAsIDisposable(object subscriber)
	{
		throw new NotImplementedException();
	}

	public void SubscribeToAction<TAction>(object subscriber, Action<TAction> callback)
	{
	}

	public void UnsubscribeFromAllActions(object subscriber)
	{
	}
}
