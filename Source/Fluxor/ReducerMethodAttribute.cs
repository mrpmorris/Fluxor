using System;

namespace Fluxor
{
	///	<summary>
	///		Identifies a method as Reducer to modify state in response to an action. This is an
	///		alternative to using <see cref="IReducer{TState}"/> or <see cref="Reducer{TState, TAction}"/>.
	///		<para>
	///			The format of the method must be ({StateType} originalState, {ActionType} action) => {StateType} newState
	///		</para>
	///	</summary>
	///	<example>
	///		public static class AClassWithOneOrMoreReducers
	///		{
	///			[ReducerMethod]
	///			public CounterState ReduceIncrementCounterAction(CounterState state, IncrementCounterAction action) =>
	///				new CounterState(state.ClickCount + 1);
	///			
	///			[ReducerMethod]
	///			public CounterState ReduceDecrementCounterAction(CounterState state, DecrementCounterAction action) =>
	///				new CounterState(state.ClickCount - 1);
	///		}
	///	</example>
	///	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public class ReducerMethodAttribute : Attribute
	{
	}
}
