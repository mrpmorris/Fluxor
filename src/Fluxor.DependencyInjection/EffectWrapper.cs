using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Fluxor.DependencyInjection
{
	internal class EffectWrapper<TAction> : IEffect
	{
		private delegate Task HandleAsyncHandler(TAction action, IDispatcher dispatcher);
		private readonly HandleAsyncHandler HandleAsync;

		Task IEffect.HandleAsync(object action, IDispatcher dispatcher) => HandleAsync((TAction)action, dispatcher);
		bool IEffect.ShouldReactToAction(object action) => action is TAction;

		public EffectWrapper(object effectHostInstance, MethodInfo methodInfo)
		{
			if (effectHostInstance == null)
			{
				// Static method
				HandleAsync = (HandleAsyncHandler)
					Delegate.CreateDelegate(
						type: typeof(HandleAsyncHandler),
						method: methodInfo);
			}
			else
			{
				// Instance method
				HandleAsync = (HandleAsyncHandler)
					Delegate.CreateDelegate(
						type: typeof(HandleAsyncHandler),
						firstArgument: effectHostInstance,
						method: methodInfo);
			}
		}
	}
}
