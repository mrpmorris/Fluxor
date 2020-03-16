using System;
using System.Reflection;

namespace Fluxor.DependencyInjection
{
	internal class ReducerWrapper<TState, TAction> : IReducer<TState>
	{
		private delegate TState ReduceHandler(TState state, TAction action);
		private readonly ReduceHandler Reduce;

		TState IReducer<TState>.Reduce(TState state, object action) => Reduce(state, (TAction)action);
		bool IReducer<TState>.ShouldReduceStateForAction(object action) => action is TAction;

		public ReducerWrapper(object reducerHostInstance, MethodInfo methodInfo)
		{
			if (reducerHostInstance == null)
			{
				// Static method
				Reduce = (ReduceHandler)
					Delegate.CreateDelegate(
						type: typeof(ReduceHandler),
						method: methodInfo);
			}
			else
			{
				// Instance method
				Reduce = (ReduceHandler)
					Delegate.CreateDelegate(
						type: typeof(ReduceHandler),
						firstArgument: reducerHostInstance,
						method: methodInfo);
			}
		}
	}
}
