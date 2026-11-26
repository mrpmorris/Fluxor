using Fluxor.DependencyInjection.Wrappers;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Fluxor.DependencyInjection.Generated;

/// <summary>
/// Factory methods used by generated code to create reducers, effects and features without reflection.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class GeneratedWrappers
{
	/// <summary>Creates a reducer that reduces actions of type <typeparamref name="TAction"/>.</summary>
	/// <param name="reduce">The reduce delegate</param>
	public static IReducer<TState> Reducer<TState, TAction>(Func<TState, TAction, TState> reduce)
		=> new ReducerWrapper<TState, TAction>(reduce);

	/// <summary>Creates a reducer for actions of type <typeparamref name="TAction"/> whose method takes no action parameter.</summary>
	/// <param name="reduce">The reduce delegate</param>
	public static IReducer<TState> ReducerWithoutAction<TState, TAction>(Func<TState, TState> reduce)
		=> new ReducerWrapper<TState, TAction>(reduce);

	/// <summary>Creates an effect that reacts to actions of type <typeparamref name="TAction"/>.</summary>
	/// <param name="handle">The handle delegate</param>
	public static IEffect Effect<TAction>(Func<TAction, IDispatcher, Task> handle)
		=> new EffectWrapper<TAction>(handle);

	/// <summary>Creates an effect for actions of type <typeparamref name="TAction"/> whose method takes no action parameter.</summary>
	/// <param name="handle">The handle delegate</param>
	public static IEffect EffectWithoutAction<TAction>(Func<IDispatcher, Task> handle)
		=> new EffectWrapper<TAction>(handle);

	/// <summary>Creates a feature for a state decorated with FeatureState.</summary>
	/// <param name="name">The feature name, or null to use the full name of <typeparamref name="TState"/></param>
	/// <param name="maximumStateChangedNotificationsPerSecond">Maximum state changed notifications per second</param>
	/// <param name="debuggerBrowsable">Whether the feature is visible to debuggers</param>
	/// <param name="createInitialState">Creates the initial state</param>
	public static IFeature<TState> FeatureState<TState>(
		string name,
		byte maximumStateChangedNotificationsPerSecond,
		bool debuggerBrowsable,
		Func<TState> createInitialState)
		=> new FeatureStateWrapper<TState>(
			name,
			maximumStateChangedNotificationsPerSecond,
			debuggerBrowsable,
			createInitialState);

	/// <summary>
	/// Creates a feature for a state decorated with FeatureState, whose initial state is created by a factory
	/// that generated code shares between state types.
	/// </summary>
	/// <param name="name">The feature name, or null to use the full name of <typeparamref name="TState"/></param>
	/// <param name="maximumStateChangedNotificationsPerSecond">Maximum state changed notifications per second</param>
	/// <param name="debuggerBrowsable">Whether the feature is visible to debuggers</param>
	/// <param name="createInitialState">Creates the initial state (a <typeparamref name="TState"/>), given <paramref name="stateIndex"/></param>
	/// <param name="stateIndex">The value passed to <paramref name="createInitialState"/></param>
	public static IFeature<TState> FeatureState<TState>(
		string name,
		byte maximumStateChangedNotificationsPerSecond,
		bool debuggerBrowsable,
		Func<int, object> createInitialState,
		int stateIndex)
		=> new FeatureStateWrapper<TState>(
			name,
			maximumStateChangedNotificationsPerSecond,
			debuggerBrowsable,
			createInitialState,
			stateIndex);
}
