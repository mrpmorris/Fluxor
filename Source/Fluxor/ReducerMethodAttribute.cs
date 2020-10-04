using System;

namespace Fluxor
{
	///	<summary>
	///		Identifies a method as Reducer to modify state in response to an action. This is an
	///		alternative to using <see cref="IReducer{TState}"/> or <see cref="Reducer{TState, TAction}"/>.
	///		<para>
	///			When no ActionType is specified <see cref="ReducerMethodAttribute.ReducerMethodAttribute"/> then the method signature must be
	///				({StateType} originalState, {ActionType} action) => {StateType} newState
	///		</para>
	///		<para>
	///			When an ActionType is specified <see cref="ReducerMethodAttribute.ReducerMethodAttribute(Type)"/> then the method signature must be
	///				({StateType} originalState) => {StateType} newState
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
	///
	///			// or
	///
	///			[ReducerMethod(typeof(IncrementCounterAction))]
	///			public CounterState ReduceIncrementCounterAction(CounterState state) =>
	///				new CounterState(state.ClickCount + 1);
	///
	///			[ReducerMethod(typeof(DecrementCounterAction))]
	///			public CounterState ReduceDecrementCounterAction(CounterState state) =>
	///				new CounterState(state.ClickCount - 1);
	///		}
	///	</example>
	///	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public class ReducerMethodAttribute : Attribute
	{
		public Type ActionType { get; private set; }

		///	<summary>
		///		Identifies a method with signature (SomeState originalState, object Action) => SomeState as a reducer method
		///	</summary>
		public ReducerMethodAttribute() { }

		///	<summary>
		///		Identifies a method with signature (SomeState originalState) => SomeState as an effect method
		///	</summary>
		///	<param name="actionType>
		///		The type of the action that triggers this reducer
		///	</param>
		public ReducerMethodAttribute(Type actionType)
		{
			ActionType = actionType ?? throw new ArgumentNullException(nameof(actionType));
		}
	}
}
