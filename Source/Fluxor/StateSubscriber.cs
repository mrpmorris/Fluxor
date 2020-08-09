using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GetStateDelegate = System.Func<object, Fluxor.IState>;

namespace Fluxor
{
	/// <summary>
	/// A utility class that automaticaly subscribes to all <see cref="IState{TState}"/> properties
	/// on a specific object
	/// </summary>
	public static class StateSubscriber
	{
		private static readonly ConcurrentDictionary<Type, IEnumerable<GetStateDelegate>> ValueDelegatesByType;

		static StateSubscriber()
		{
			ValueDelegatesByType = new ConcurrentDictionary<Type, IEnumerable<GetStateDelegate>>();
		}

		/// <summary>
		/// Subscribes to all <see cref="IState{TState}"/> properties on the specified <paramref name="subject"/>
		/// to ensure <paramref name="callback"/> is called whenever a state is modified
		/// </summary>
		/// <param name="subject">The object to scan for <see cref="IState{TState}"/> properties.</param>
		/// <param name="callback">The action to execute when one of the states are modified</param>
		/// <returns></returns>
		public static IDisposable Subscribe(object subject, Action<IState> callback)
		{
			if (subject == null)
				throw new ArgumentNullException(nameof(subject));
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));

			IEnumerable<GetStateDelegate> getStateDelegates = GetStateDelegatesForType(subject.GetType());
			var subscriptions = new List<(IState State, EventHandler Handler)>();
			foreach (GetStateDelegate getState in getStateDelegates)
			{
				var state = (IState)getState(subject);
				var handler = new EventHandler((s, a) => callback(state));

				subscriptions.Add((state, handler));
				state.StateChanged += handler;
			}
			return new DisposableCallback(
				id: $"{nameof(StateSubscriber)}.{nameof(Subscribe)}",
				() =>
					{
						foreach (var subscription in subscriptions)
							subscription.State.StateChanged -= subscription.Handler;
					});
		}

		private static IEnumerable<PropertyInfo> GetStateProperties(Type t) =>
			t == typeof(object)
			? Enumerable.Empty<PropertyInfo>()
			: GetStateProperties(t.BaseType)
				.Union(
					t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
						.Where(p => p.PropertyType.IsGenericType)
						.Where(p => p.PropertyType.GetGenericTypeDefinition() == typeof(IState<>))
				);

		private static IEnumerable<GetStateDelegate> GetStateDelegatesForType(Type type)
		{
			return ValueDelegatesByType.GetOrAdd(type, _ =>
			{
				var delegates = new List<GetStateDelegate>();
				IEnumerable<PropertyInfo> stateProperties = GetStateProperties(type);

				foreach (PropertyInfo currentProperty in stateProperties)
				{
					Type stateType = currentProperty.PropertyType.GetGenericArguments()[0];
					Type iStateType = typeof(IState<>).MakeGenericType(stateType);
					Type getterMethod = typeof(Func<,>).MakeGenericType(type, iStateType);
					Delegate stronglyTypedDelegate = Delegate.CreateDelegate(getterMethod, currentProperty.GetGetMethod(true));
					var getValueDelegate = new GetStateDelegate(x => (IState)stronglyTypedDelegate.DynamicInvoke(x));
					delegates.Add(getValueDelegate);
				}

				return delegates;
			});
		}
	}
}
