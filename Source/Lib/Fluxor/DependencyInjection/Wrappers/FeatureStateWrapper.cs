using System;

namespace Fluxor.DependencyInjection.Wrappers;

internal class FeatureStateWrapper<TState> : Feature<TState>
{
	private readonly string Name;
	private readonly Func<object> CreateInitialStateFunc;
	// Generated code's shared initial state factory and the state index to call it with: no closure
	private readonly Func<int, object> CreateInitialStateFromIndex;
	private readonly int StateIndex;

	public FeatureStateWrapper(FeatureStateInfo info)
	{
		Name = info.FeatureStateAttribute.Name ?? typeof(TState).FullName;
		MaximumStateChangedNotificationsPerSecond = info.FeatureStateAttribute.MaximumStateChangedNotificationsPerSecond;
		DebuggerBrowsable = info.FeatureStateAttribute.DebuggerBrowsable;
		CreateInitialStateFunc = info.CreateInitialStateFunc;
	}

	public FeatureStateWrapper(
		string name,
		byte maximumStateChangedNotificationsPerSecond,
		bool debuggerBrowsable,
		Func<TState> createInitialState)
	{
		Name = name ?? typeof(TState).FullName;
		MaximumStateChangedNotificationsPerSecond = maximumStateChangedNotificationsPerSecond;
		DebuggerBrowsable = debuggerBrowsable;
		CreateInitialStateFunc = () => createInitialState();
	}

	public FeatureStateWrapper(
		string name,
		byte maximumStateChangedNotificationsPerSecond,
		bool debuggerBrowsable,
		Func<int, object> createInitialState,
		int stateIndex)
	{
		if (createInitialState is null)
			throw new ArgumentNullException(nameof(createInitialState));
		Name = name ?? typeof(TState).FullName;
		MaximumStateChangedNotificationsPerSecond = maximumStateChangedNotificationsPerSecond;
		DebuggerBrowsable = debuggerBrowsable;
		CreateInitialStateFromIndex = createInitialState;
		StateIndex = stateIndex;
	}

	public override string GetName() => Name;

	protected override TState GetInitialState() =>
		CreateInitialStateFromIndex is not null
		? (TState)CreateInitialStateFromIndex(StateIndex)
		: (TState)CreateInitialStateFunc();
}
