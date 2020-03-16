using System;
using System.Threading.Tasks;

namespace Fluxor.UnitTests.SupportFiles
{
	public class EffectThatEmitsActions<TTriggerAction> : Effect<TTriggerAction>
	{
		public readonly object[] ActionsToEmit;

		public EffectThatEmitsActions(object[] actionsToEmit)
		{
			ActionsToEmit = actionsToEmit ?? Array.Empty<object>();
		}
		protected override Task HandleAsync(TTriggerAction action, IDispatcher dispatcher)
		{
			foreach (object actionToEmit in ActionsToEmit)
				dispatcher.Dispatch(actionToEmit);
			return Task.CompletedTask;
		}
	}
}
