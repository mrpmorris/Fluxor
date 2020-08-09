using System;

namespace Fluxor
{
	/// <summary>
	/// Provides a mechanism for subscribing to the store to receive notifications when
	/// specific action types are dispatched
	/// </summary>
	public interface IActionSubscriber
	{
		/// <summary>
		/// Subscribes to be notified whenever a specific action is dispatched
		/// </summary>
		/// <typeparam name="TAction">The type of action (and its descendants) to be notified of</typeparam>
		/// <param name="subscriber">The instance that is subscribing to notifications</param>
		/// <param name="callback">The action to execute whenever a qualifying action is dispatched</param>
		void SubscribeToAction<TAction>(object subscriber, Action<TAction> callback);
		/// <summary>
		/// Removes all subscriptions for the specified subscriber. 
		/// </summary>
		/// <remarks>
		/// Subscriptions should be removed when the subscriber's <see cref="IDisposable.Dispose"/>
		/// method is executed in order to avoid memory leaks.
		/// </remarks>
		/// <param name="subscriber">The instance that no longer wishes to receive notifications</param>
		void UnsubscribeFromAllActions(object subscriber);
		/// <summary>
		/// Returns an <see cref="IDisposable"/> that can be used to remove all subscriptions for the specified
		/// subscriber. When <see cref="IDisposable.Dispose"/> is executed on the result then
		/// <see cref="IActionSubscriber.UnsubscribeFromAllActions(object)"/> will be executed for the specified
		/// subscriber.
		/// </summary>
		/// <remarks>
		/// Subscriptions should be removed when the subscriber's <see cref="IDisposable.Dispose"/>
		/// method is executed in order to avoid memory leaks.
		/// </remarks>
		/// <param name="subscriber">The subscriber to unsubscribe</param>
		/// <returns></returns>
		IDisposable GetActionUnsubscriberAsIDisposable(object subscriber);
	}
}
