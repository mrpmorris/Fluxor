using System;
using System.Threading.Tasks;

namespace Fluxor.DependencyInjection
{
	internal class EffectWrapper<TAction> : IEffect
	{
		private delegate Task HandleWithActionParameterAsyncHandler(TAction action, IDispatcher dispatcher);
		private delegate Task HandleWithoutActionParameterAsyncHandler(IDispatcher dispatcher);
		private readonly HandleWithActionParameterAsyncHandler HandleAsync;

		Task IEffect.HandleAsync(object action, IDispatcher dispatcher) => HandleAsync((TAction)action, dispatcher);
		bool IEffect.ShouldReactToAction(object action) => action is TAction;

		public EffectWrapper(object effectHostInstance, DiscoveredEffectMethod discoveredEffectMethod)
		{
			HandleAsync =
				discoveredEffectMethod.RequiresActionParameterInMethod
				? CreateHandlerWithActionParameter(effectHostInstance, discoveredEffectMethod)
				: WrapEffectWithoutActionParameter(effectHostInstance, discoveredEffectMethod); ;
		}

		private static HandleWithActionParameterAsyncHandler WrapEffectWithoutActionParameter(
			object effectHostInstance,
			DiscoveredEffectMethod discoveredEffectMethod)
		{
			HandleWithoutActionParameterAsyncHandler handler = CreateHandlerWithoutActionParameter(
				effectHostInstance,
				discoveredEffectMethod);

			return new HandleWithActionParameterAsyncHandler((action, dispatcher) => handler.Invoke(dispatcher));
		}

		private static HandleWithActionParameterAsyncHandler CreateHandlerWithActionParameter(
			object effectHostInstance,
			DiscoveredEffectMethod discoveredEffectMethod)
			=>
				effectHostInstance == null
				? CreateStaticHandlerWithActionParameter(discoveredEffectMethod)
				: CreateInstanceHandlerWithActionParameter(effectHostInstance, discoveredEffectMethod);

		private static HandleWithActionParameterAsyncHandler CreateStaticHandlerWithActionParameter(
			DiscoveredEffectMethod discoveredEffectMethod)
			=>
				(HandleWithActionParameterAsyncHandler)
					Delegate.CreateDelegate(
						type: typeof(HandleWithActionParameterAsyncHandler),
						method: discoveredEffectMethod.MethodInfo);

		private static HandleWithActionParameterAsyncHandler CreateInstanceHandlerWithActionParameter(
			object effectHostInstance,
			DiscoveredEffectMethod discoveredEffectMethod)
			=>
				(HandleWithActionParameterAsyncHandler)
					Delegate.CreateDelegate(
						type: typeof(HandleWithActionParameterAsyncHandler),
						firstArgument: effectHostInstance,
						method: discoveredEffectMethod.MethodInfo);

		private static HandleWithoutActionParameterAsyncHandler CreateHandlerWithoutActionParameter(
			object effectHostInstance,
			DiscoveredEffectMethod discoveredEffectMethod)
			=>
				effectHostInstance == null
				? CreateStaticHandlerWithoutActionParameter(discoveredEffectMethod)
				: CreateInstanceHandlerWithoutActionParameter(effectHostInstance, discoveredEffectMethod);

		private static HandleWithoutActionParameterAsyncHandler CreateStaticHandlerWithoutActionParameter(
			DiscoveredEffectMethod discoveredEffectMethod)
			=>
				(HandleWithoutActionParameterAsyncHandler)
					Delegate.CreateDelegate(
						type: typeof(HandleWithoutActionParameterAsyncHandler),
						method: discoveredEffectMethod.MethodInfo);

		private static HandleWithoutActionParameterAsyncHandler CreateInstanceHandlerWithoutActionParameter(
			object effectHostInstance,
			DiscoveredEffectMethod discoveredEffectMethod)
			=>
				(HandleWithoutActionParameterAsyncHandler)
					Delegate.CreateDelegate(
						type: typeof(HandleWithoutActionParameterAsyncHandler),
						firstArgument: effectHostInstance,
						method: discoveredEffectMethod.MethodInfo);
	}
}
