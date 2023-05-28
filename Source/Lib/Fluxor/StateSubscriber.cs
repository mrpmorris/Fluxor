using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GetStateChangedPropertyDelegate = System.Func<object, Fluxor.IStateChangedNotifier>;

namespace Fluxor
{
	/// <summary>
	/// A utility class that automaticaly subscribes to all <see cref="IStateChangedNotifier"/> properties
	/// on a specific object
	/// </summary>
	public static class StateSubscriber
	{
		private static readonly ConcurrentDictionary<Type, IEnumerable<GetStateChangedPropertyDelegate>> ValueDelegatesByType;

		static StateSubscriber()
		{
			ValueDelegatesByType = new ConcurrentDictionary<Type, IEnumerable<GetStateChangedPropertyDelegate>>();
		}

		/// <summary>
		/// Subscribes to all <see cref="IStateChangedNotifier"/> properties on the specified <paramref name="subject"/>
		/// to ensure <paramref name="callback"/> is called whenever a state is modified
		/// </summary>
		/// <param name="subject">The object to scan for <see cref="IStateChangedNotifier"/> properties.</param>
		/// <param name="callback">The action to execute when one of the states are modified</param>
		/// <returns></returns>
		public static IDisposable Subscribe(object subject, Action<IStateChangedNotifier> callback)
		{
			if (subject is null)
				throw new ArgumentNullException(nameof(subject));
			if (callback is null)
				throw new ArgumentNullException(nameof(callback));

			IEnumerable<GetStateChangedPropertyDelegate> getStateChangedNotifierPropertyDelegates =
				GetStateChangedNotifierPropertyDelegatesForType(subject.GetType());
			var subscriptions = new List<(IStateChangedNotifier StateChangedNotifier, EventHandler Handler)>();
			
			foreach (GetStateChangedPropertyDelegate getStateChangedNotifierPropertyValue in getStateChangedNotifierPropertyDelegates)
			{
				IStateChangedNotifier stateChangedNotifier = getStateChangedNotifierPropertyValue(subject);
				var handler = new EventHandler((s, a) => callback(stateChangedNotifier));

				subscriptions.Add((stateChangedNotifier, handler));
				stateChangedNotifier.StateChanged += handler;
			}

			return new DisposableCallback(
				id: $"{nameof(StateSubscriber)}.{nameof(Subscribe)} / {subject.GetType().FullName}",
				() =>
					{
						foreach (var subscription in subscriptions)
							subscription.StateChangedNotifier.StateChanged -= subscription.Handler;
					});
		}

		private static IEnumerable<PropertyInfo> GetStateChangedNotifierProperties(Type t) =>
			t == typeof(object)
			? Enumerable.Empty<PropertyInfo>()
			: GetStateChangedNotifierProperties(t.BaseType)
				.Union(
					t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
						.Where(p => typeof(IStateChangedNotifier).IsAssignableFrom(p.PropertyType))
				);

		private static IEnumerable<GetStateChangedPropertyDelegate> GetStateChangedNotifierPropertyDelegatesForType(Type type)
		{
			return ValueDelegatesByType.GetOrAdd(type, _ =>
			{
				var delegates = new List<GetStateChangedPropertyDelegate>();
				IEnumerable<PropertyInfo> stateChangedNotifierProperties = GetStateChangedNotifierProperties(type);

				foreach (PropertyInfo currentProperty in stateChangedNotifierProperties)
				{
					Type getterMethod = typeof(Func<,>).MakeGenericType(type, currentProperty.PropertyType);
					var stronglyTypedDelegate = Delegate.CreateDelegate(getterMethod, currentProperty.GetGetMethod(true));
					var getValueDelegate = new GetStateChangedPropertyDelegate(
						x => (IStateChangedNotifier)stronglyTypedDelegate.DynamicInvoke(x));
					delegates.Add(getValueDelegate);
				}

				return delegates;
			});
		}
	}
}
