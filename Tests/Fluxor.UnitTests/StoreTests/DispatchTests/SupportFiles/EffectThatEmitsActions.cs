using System;
using System.Threading.Tasks;

namespace Fluxor.UnitTests.StoreTests.DispatchTests.SupportFiles
{
	public class EffectThatEmitsActions : Effect<TestAction>
	{
		public readonly object[] ActionsToEmit;

		public EffectThatEmitsActions(object[] actionsToEmit)
		{
			ActionsToEmit = actionsToEmit ?? Array.Empty<object>();
		}
		protected override Task HandleAsync(TestAction action, IDispatcher dispatcher)
		{
			foreach (object actionToEmit in ActionsToEmit)
				dispatcher.Dispatch(actionToEmit);
			return Task.CompletedTask;
		}
	}
}
