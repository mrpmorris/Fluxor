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

		public ReducerWrapper() { }
		public ReducerWrapper(object reducerHostInstance, DiscoveredReducerMethod discoveredReducerMethod)
		{
			Reduce =
				discoveredReducerMethod.RequiresActionParameterInMethod
				? CreateReducerWithActionParameter(reducerHostInstance, discoveredReducerMethod)
				: WrapReducerWithoutActionParameter(reducerHostInstance, discoveredReducerMethod);
		}

		private static ReduceWithActionParameterHandler WrapReducerWithoutActionParameter(
			object reducerHostInstance,
			DiscoveredReducerMethod discoveredReducerMethod)
		{
			ReduceWithoutActionParameterHandler handler = CreateReducerWithoutActionParameter(
				reducerHostInstance,
				discoveredReducerMethod);

			return new ReduceWithActionParameterHandler((state, action) => handler.Invoke(state));
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

		private static ReduceWithoutActionParameterHandler CreateReducerWithoutActionParameter(
			object reducerHostInstance,
			DiscoveredReducerMethod discoveredReducerMethod)
			=>
				reducerHostInstance == null
				? CreateStaticReducerWithoutActionParameter(discoveredReducerMethod)
				: CreateInstanceReducerWithoutActionParameter(reducerHostInstance, discoveredReducerMethod);

		private static ReduceWithoutActionParameterHandler CreateStaticReducerWithoutActionParameter(
			DiscoveredReducerMethod discoveredReducerMethod)
			=>
				(ReduceWithoutActionParameterHandler)
					Delegate.CreateDelegate(
						type: typeof(ReduceWithoutActionParameterHandler),
						method: discoveredReducerMethod.MethodInfo);

		private static ReduceWithoutActionParameterHandler CreateInstanceReducerWithoutActionParameter(
			object reducerHostInstance,
			DiscoveredReducerMethod discoveredReducerMethod)
			=>
				(ReduceWithoutActionParameterHandler)
					Delegate.CreateDelegate(
						type: typeof(ReduceWithoutActionParameterHandler),
						firstArgument: reducerHostInstance,
						method: discoveredReducerMethod.MethodInfo);
	}
}
