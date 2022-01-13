using Fluxor.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Fluxor
{
	internal class ActionSubscriber : IActionSubscriber
	{
		private readonly Dictionary<object, List<ActionSubscription>> SubscriptionsForInstance = new();
		private readonly Dictionary<Type, List<ActionSubscription>> SubscriptionsForType = new();
		private SpinLock SpinLock = new();


		public IDisposable GetActionUnsubscriberAsIDisposable(object subscriber) =>
			new DisposableCallback(
				id: $"{nameof(ActionSubscriber)}.{nameof(GetActionUnsubscriberAsIDisposable)}",
				action: () => UnsubscribeFromAllActions(subscriber));

		public void Notify(object action)
		{
			if (action is null)
				throw new ArgumentNullException(nameof(action));

			SpinLock.ExecuteLocked(() =>
			{
				IEnumerable<Action<object>> callbacks =
					SubscriptionsForType
						.Where(x => x.Key.IsAssignableFrom(action.GetType()))
						.SelectMany(x => x.Value)
						.Select(x => x.Callback)
						.ToArray();
				foreach (Action<object> callback in callbacks)
					callback(action);
			});
		}

		public void SubscribeToAction<TAction>(object subscriber, Action<TAction> callback)
		{
			if (subscriber is null)
				throw new ArgumentNullException(nameof(subscriber));
			if (callback is null)
				throw new ArgumentNullException(nameof(callback));

			var subscription = new ActionSubscription(
				subscriber: subscriber,
				actionType: typeof(TAction),
				callback: (object action) => callback((TAction)action));

			SpinLock.ExecuteLocked(() =>
			{
				if (!SubscriptionsForInstance.TryGetValue(subscriber, out List<ActionSubscription> instanceSubscriptions))
				{
					instanceSubscriptions = new List<ActionSubscription>();
					SubscriptionsForInstance[subscriber] = instanceSubscriptions;
				}
				instanceSubscriptions.Add(subscription);

				if (!SubscriptionsForType.TryGetValue(typeof(TAction), out List<ActionSubscription> typeSubscriptions))
				{
					typeSubscriptions = new List<ActionSubscription>();
					SubscriptionsForType[typeof(TAction)] = typeSubscriptions;
				}
				typeSubscriptions.Add(subscription);
			});
		}

		public void UnsubscribeFromAllActions(object subscriber)
		{
			if (subscriber is null)
				throw new ArgumentNullException(nameof(subscriber));

			List<ActionSubscription> instanceSubscriptions;
			SpinLock.ExecuteLocked(() =>
			{
				if (!SubscriptionsForInstance.TryGetValue(subscriber, out instanceSubscriptions))
					return;

				IEnumerable<Type> subscribedActionTypes =
					instanceSubscriptions
						.Select(x => x.ActionType)
						.Distinct();

				foreach(Type actionType in subscribedActionTypes)
				{
					List<ActionSubscription> actionTypeSubscriptions;
					if (!SubscriptionsForType.TryGetValue(actionType, out actionTypeSubscriptions))
						continue;
					SubscriptionsForType[actionType] = actionTypeSubscriptions
						.Except(instanceSubscriptions)
						.ToList();
				}
			});
		}
	}
}
