using System;

namespace Fluxor.DependencyInjection
{
	internal class ReducerWrapper<TState, TAction> : IReducer<TState>
	{
		private delegate TState ReduceWithActionParameterHandler(TState state, TAction action);
		private delegate TState ReduceWithoutActionParameterHandler(TState state);
		private readonly ReduceWithActionParameterHandler Reduce;

		TState IReducer<TState>.Reduce(TState state, object action) => Reduce(state, (TAction)action);
		bool IReducer<TState>.ShouldReduceStateForAction(object action) => action is TAction;

		public ReducerWrapper(object reducerHostInstance, DiscoveredReducerMethod discoveredReducerMethod)
		{
			Reduce =
				discoveredReducerMethod.RequiresActionParameterInMethod
				? CreateReducerWithActionParameter(reducerHostInstance, discoveredReducerMethod)
				: throw new NotImplementedException();
		}

		private static ReduceWithActionParameterHandler CreateReducerWithActionParameter(
			object reducerHostInstance,
			DiscoveredReducerMethod discoveredReducerMethod)
			=>
				reducerHostInstance == null
				? CreateStaticReducerWithActionParameter(discoveredReducerMethod)
				: CreateInstanceReducerWithActionParameter(reducerHostInstance, discoveredReducerMethod);

		private static ReduceWithActionParameterHandler CreateStaticReducerWithActionParameter(
			DiscoveredReducerMethod discoveredReducerMethod)
			=>
				(ReduceWithActionParameterHandler)
					Delegate.CreateDelegate(
						type: typeof(ReduceWithActionParameterHandler),
						method: discoveredReducerMethod.MethodInfo);

		private static ReduceWithActionParameterHandler CreateInstanceReducerWithActionParameter(
			object reducerHostInstance,
			DiscoveredReducerMethod discoveredReducerMethod)
			=>
				(ReduceWithActionParameterHandler)
					Delegate.CreateDelegate(
						type: typeof(ReduceWithActionParameterHandler),
						firstArgument: reducerHostInstance,
						method: discoveredReducerMethod.MethodInfo);
	}
}
