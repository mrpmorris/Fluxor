using System;
using System.Threading.Tasks;

namespace Fluxor.DependencyInjection.Wrappers;

internal class EffectWrapper<TAction> : IEffect
{
	private delegate Task HandleWithActionParameterAsyncHandler(TAction action, IDispatcher dispatcher);
	private delegate Task HandleWithoutActionParameterAsyncHandler(IDispatcher dispatcher);
	private readonly HandleWithActionParameterAsyncHandler HandleAsync;

	Task IEffect.HandleAsync(object action, IDispatcher dispatcher) =>
		HandleAsync is not null
		? HandleAsync((TAction)action, dispatcher)
		: HandleWithGeneratedHandler(action, dispatcher);

	// Generated code supplies a typed delegate (Handler) that is called directly: no closure, no other delegate type
	private Task HandleWithGeneratedHandler(object action, IDispatcher dispatcher) =>
		Handler is Func<TAction, IDispatcher, Task> handle
		? handle((TAction)action, dispatcher)
		: ((Func<IDispatcher, Task>)Handler)(dispatcher);
	bool IEffect.ShouldReactToAction(object action) => action is TAction;

	// The delegate supplied by generated code, kept so tests can inspect Handler.Method
	private readonly Delegate Handler;

	public EffectWrapper(Func<TAction, IDispatcher, Task> handle)
	{
		Handler = handle ?? throw new ArgumentNullException(nameof(handle));
	}

	public EffectWrapper(Func<IDispatcher, Task> handle)
	{
		Handler = handle ?? throw new ArgumentNullException(nameof(handle));
	}

	public EffectWrapper(object effectHostInstance, EffectMethodInfo effectMethodInfos)
	{
		HandleAsync =
			effectMethodInfos.RequiresActionParameterInMethod
			? CreateHandlerWithActionParameter(effectHostInstance, effectMethodInfos)
			: WrapEffectWithoutActionParameter(effectHostInstance, effectMethodInfos); ;
	}

	private static HandleWithActionParameterAsyncHandler WrapEffectWithoutActionParameter(
		object effectHostInstance,
		EffectMethodInfo effectMethodInfos)
	{
		HandleWithoutActionParameterAsyncHandler handler = CreateHandlerWithoutActionParameter(
			effectHostInstance,
			effectMethodInfos);

		return new HandleWithActionParameterAsyncHandler((action, dispatcher) => handler.Invoke(dispatcher));
	}

	private static HandleWithActionParameterAsyncHandler CreateHandlerWithActionParameter(
		object effectHostInstance,
		EffectMethodInfo effectMethodInfos)
		=>
			effectHostInstance is null
			? CreateStaticHandlerWithActionParameter(effectMethodInfos)
			: CreateInstanceHandlerWithActionParameter(effectHostInstance, effectMethodInfos);

	private static HandleWithActionParameterAsyncHandler CreateStaticHandlerWithActionParameter(
		EffectMethodInfo effectMethodInfo)
		=>
			(HandleWithActionParameterAsyncHandler)
				Delegate.CreateDelegate(
					type: typeof(HandleWithActionParameterAsyncHandler),
					method: effectMethodInfo.MethodInfo);

	private static HandleWithActionParameterAsyncHandler CreateInstanceHandlerWithActionParameter(
		object effectHostInstance,
		EffectMethodInfo effectMethodInfo)
		=>
			(HandleWithActionParameterAsyncHandler)
				Delegate.CreateDelegate(
					type: typeof(HandleWithActionParameterAsyncHandler),
					firstArgument: effectHostInstance,
					method: effectMethodInfo.MethodInfo);

	private static HandleWithoutActionParameterAsyncHandler CreateHandlerWithoutActionParameter(
		object effectHostInstance,
		EffectMethodInfo effectMethodInfo)
		=>
			effectHostInstance is null
			? CreateStaticHandlerWithoutActionParameter(effectMethodInfo)
			: CreateInstanceHandlerWithoutActionParameter(effectHostInstance, effectMethodInfo);

	private static HandleWithoutActionParameterAsyncHandler CreateStaticHandlerWithoutActionParameter(
		EffectMethodInfo effectMethodInfo)
		=>
			(HandleWithoutActionParameterAsyncHandler)
				Delegate.CreateDelegate(
					type: typeof(HandleWithoutActionParameterAsyncHandler),
					method: effectMethodInfo.MethodInfo);

	private static HandleWithoutActionParameterAsyncHandler CreateInstanceHandlerWithoutActionParameter(
		object effectHostInstance,
		EffectMethodInfo effectMethodInfo)
		=>
			(HandleWithoutActionParameterAsyncHandler)
				Delegate.CreateDelegate(
					type: typeof(HandleWithoutActionParameterAsyncHandler),
					firstArgument: effectHostInstance,
					method: effectMethodInfo.MethodInfo);
}
