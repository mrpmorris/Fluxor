using System;

namespace Fluxor.DependencyInjection.Wrappers;

internal class ReducerWrapper<TState, TAction> : IReducer<TState>
{
	private delegate TState ReduceWithActionParameterHandler(TState state, TAction action);
	private delegate TState ReduceWithoutActionParameterHandler(TState state);
	private readonly ReduceWithActionParameterHandler Reduce;

	// Generated code supplies a typed delegate that is called directly: no closure, no other delegate type
	private readonly Func<TState, TAction, TState> GeneratedReduce;
	private readonly Func<TState, TState> GeneratedReduceWithoutAction;

	TState IReducer<TState>.Reduce(TState state, object action)
	{
		if (Reduce is not null)
			return Reduce(state, (TAction)action);
		if (GeneratedReduce is not null)
			return GeneratedReduce(state, (TAction)action);
		return GeneratedReduceWithoutAction(state);
	}
	bool IReducer<TState>.ShouldReduceStateForAction(object action) => action is TAction;

	// The delegate supplied by generated code, kept so tests can inspect Handler.Method
	private readonly Delegate Handler;

	public ReducerWrapper() { }

	public ReducerWrapper(Func<TState, TAction, TState> reduce)
	{
		Handler = reduce ?? throw new ArgumentNullException(nameof(reduce));
		GeneratedReduce = reduce;
	}

	public ReducerWrapper(Func<TState, TState> reduce)
	{
		Handler = reduce ?? throw new ArgumentNullException(nameof(reduce));
		GeneratedReduceWithoutAction = reduce;
	}

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
			reducerHostInstance is null
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
			reducerHostInstance is null
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
