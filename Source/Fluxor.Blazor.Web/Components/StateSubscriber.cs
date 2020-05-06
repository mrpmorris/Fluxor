using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GetStateDelegate = System.Func<object, Fluxor.IState>;

namespace Fluxor.Blazor.Web.Components
{
	internal static class StateSubscriber
	{
		private static readonly ConcurrentDictionary<Type, IEnumerable<GetStateDelegate>> ValueDelegatesForType;

		static StateSubscriber()
		{
			ValueDelegatesForType = new ConcurrentDictionary<Type, IEnumerable<GetStateDelegate>>();
		}

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
			return ValueDelegatesForType.GetOrAdd(type, _ =>
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
