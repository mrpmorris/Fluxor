using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fluxor.UnitTests.StoreTests.ThreadingTests.DispatchWhileInitializingTests.SupportFiles;

public class EffectThatEmitsActions : Effect<StoreInitializedAction>
{
	public readonly object[] ActionsToEmit;

	public EffectThatEmitsActions(object[] actionsToEmit)
	{
		ActionsToEmit = actionsToEmit ?? Array.Empty<object>();
	}
	public override async Task HandleAsync(StoreInitializedAction action, IDispatcher dispatcher)
	{
		Thread.Sleep(500);
		foreach (object actionToEmit in ActionsToEmit)
			await dispatcher.DispatchAsync(actionToEmit);
	}
}
