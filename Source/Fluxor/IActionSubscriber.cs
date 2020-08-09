using System;

namespace Fluxor
{
	//TODO: PeteM - XML Comments
	public interface IActionSubscriber
	{
		void SubscribeToAction<TAction>(object subscriber, Action<TAction> callback);
		void UnsubscribeFromAllActions(object subscriber);
		IDisposable GetActionUnsubscriberAsIDisposable(object subscriber);
	}
}
