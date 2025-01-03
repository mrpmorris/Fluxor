using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GetStateChangedFieldDelegate = System.Func<object, Fluxor.IStateChangedNotifier>;

namespace Fluxor;

public static class StateSubscriber
{
	private static readonly ConcurrentDictionary<Type, IEnumerable<GetStateChangedFieldDelegate>> ValueDelegatesByType;

	static StateSubscriber()
		=> ValueDelegatesByType = new ConcurrentDictionary<Type, IEnumerable<GetStateChangedFieldDelegate>>();

	/// <summary>
	/// Subscribes to all <see cref="IStateChangedNotifier"/> fields on the specified <paramref name="subject"/>
	/// to ensure <paramref name="callback"/> is called whenever a state is modified.
	/// </summary>
	/// <param name="subject">The object to scan for <see cref="IStateChangedNotifier"/> fields.</param>
	/// <param name="callback">The action to execute when one of the states are modified.</param>
	/// <returns></returns>
	public static IDisposable Subscribe(object subject, Action<IStateChangedNotifier> callback)
	{
		if (subject is null)
			throw new ArgumentNullException(nameof(subject));
		if (callback is null)
			throw new ArgumentNullException(nameof(callback));

		IEnumerable<GetStateChangedFieldDelegate> getStateChangedNotifierFieldDelegates =
			GetStateChangedNotifierFieldDelegatesForType(subject.GetType());

		var subscriptions = new List<(IStateChangedNotifier StateChangedNotifier, EventHandler Handler)>();

		foreach (GetStateChangedFieldDelegate getStateChangedNotifierFieldValue in getStateChangedNotifierFieldDelegates)
		{
			IStateChangedNotifier stateChangedNotifier = getStateChangedNotifierFieldValue(subject);
			var handler = new EventHandler((_, __) => callback(stateChangedNotifier));

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

	private static IEnumerable<FieldInfo> GetStateChangedNotifierFields(Type t) =>
		t == typeof(object)
		? Enumerable.Empty<FieldInfo>()
		: GetStateChangedNotifierFields(t.BaseType)
			.Union(
				t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
					.Where(f => typeof(IStateChangedNotifier).IsAssignableFrom(f.FieldType))
			);

	private static IEnumerable<GetStateChangedFieldDelegate> GetStateChangedNotifierFieldDelegatesForType(Type type)
	{
		return ValueDelegatesByType.GetOrAdd(type, _ =>
		{
			var delegates = new List<GetStateChangedFieldDelegate>();
			IEnumerable<FieldInfo> stateChangedNotifierFields = GetStateChangedNotifierFields(type);

			foreach (FieldInfo currentField in stateChangedNotifierFields)
			{
				var getValueDelegate = new GetStateChangedFieldDelegate(
					x => (IStateChangedNotifier)currentField.GetValue(x)!);
				delegates.Add(getValueDelegate);
			}

			return delegates;
		});
	}
}
