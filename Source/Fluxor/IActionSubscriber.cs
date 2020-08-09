using System;

namespace Fluxor
{
	//TODO: PeteM - XML Comments
	public interface IActionSubscriber
	{
		void SubscribeToAction<TAction>(object subscriber, Action<TAction> callback);
		void CancelActionSubscriptions(object subscriber);
		IDisposable GetIDisposableForActionSubscriptions(object subscriber);
	}
}
