using System.Threading.Tasks;

namespace Fluxor
{
	/// <summary>
	/// A generic class that can be used as a base for effects.
	/// </summary>
	/// <typeparam name="TTriggerAction"></typeparam>
	public abstract class Effect<TTriggerAction> : IEffect
	{
		/// <summary>
		/// <see cref="IEffect.HandleAsync(object, IDispatcher)"/>
		/// </summary>
		protected abstract Task HandleAsync(TTriggerAction action, IDispatcher dispatcher);

		/// <summary>
		/// <see cref="IEffect.ShouldReactToAction(object)"/>
		/// </summary>
		public bool ShouldReactToAction(object action)
		{
			return action is TTriggerAction;
		}

		Task IEffect.HandleAsync(object action, IDispatcher dispatcher)
		{
			return HandleAsync((TTriggerAction)action, dispatcher);
		}
	}
}
