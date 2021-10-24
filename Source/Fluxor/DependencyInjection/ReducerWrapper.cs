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
		public ReducerWrapper(
			object reducerHostInstance,
			ReducerMethodInfo reducerMethodInfos)
		{
			Reduce =
				reducerMethodInfos.RequiresActionParameterInMethod
				? CreateReducerWithActionParameter(reducerHostInstance, reducerMethodInfos)
				: WrapReducerWithoutActionParameter(reducerHostInstance, reducerMethodInfos);
		}

		private static ReduceWithActionParameterHandler WrapReducerWithoutActionParameter(
			object reducerHostInstance,
			ReducerMethodInfo reducerMethodInfo)
		{
			ReduceWithoutActionParameterHandler handler = CreateReducerWithoutActionParameter(
				reducerHostInstance,
				reducerMethodInfo);

			return new ReduceWithActionParameterHandler((state, action) => handler.Invoke(state));
		}

		private static ReduceWithActionParameterHandler CreateReducerWithActionParameter(
			object reducerHostInstance,
			ReducerMethodInfo reducerMethodInfo)
			=>
				reducerHostInstance == null
				? CreateStaticReducerWithActionParameter(reducerMethodInfo)
				: CreateInstanceReducerWithActionParameter(reducerHostInstance, reducerMethodInfo);

		private static ReduceWithActionParameterHandler CreateStaticReducerWithActionParameter(
			ReducerMethodInfo reducerMethodInfo)
			=>
				(ReduceWithActionParameterHandler)
					Delegate.CreateDelegate(
						type: typeof(ReduceWithActionParameterHandler),
						method: reducerMethodInfo.MethodInfo);

		private static ReduceWithActionParameterHandler CreateInstanceReducerWithActionParameter(
			object reducerHostInstance,
			ReducerMethodInfo reducerMethodInfo)
			=>
				(ReduceWithActionParameterHandler)
					Delegate.CreateDelegate(
						type: typeof(ReduceWithActionParameterHandler),
						firstArgument: reducerHostInstance,
						method: reducerMethodInfo.MethodInfo);

		private static ReduceWithoutActionParameterHandler CreateReducerWithoutActionParameter(
			object reducerHostInstance,
			ReducerMethodInfo reducerMethodInfo)
			=>
				reducerHostInstance == null
				? CreateStaticReducerWithoutActionParameter(reducerMethodInfo)
				: CreateInstanceReducerWithoutActionParameter(reducerHostInstance, reducerMethodInfo);

		private static ReduceWithoutActionParameterHandler CreateStaticReducerWithoutActionParameter(
			ReducerMethodInfo reducerMethodInfo)
			=>
				(ReduceWithoutActionParameterHandler)
					Delegate.CreateDelegate(
						type: typeof(ReduceWithoutActionParameterHandler),
						method: reducerMethodInfo.MethodInfo);

		private static ReduceWithoutActionParameterHandler CreateInstanceReducerWithoutActionParameter(
			object reducerHostInstance,
			ReducerMethodInfo reducerMethodInfo)
			=>
				(ReduceWithoutActionParameterHandler)
					Delegate.CreateDelegate(
						type: typeof(ReduceWithoutActionParameterHandler),
						firstArgument: reducerHostInstance,
						method: reducerMethodInfo.MethodInfo);
	}
}
