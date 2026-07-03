using System;
using System.Threading.Tasks;

namespace Fluxor.UnitTests.StoreTests.DispatchTests.SupportFiles;

public class EffectThatEmitsActions : Effect<TestAction>
{
	public readonly object[] ActionsToEmit;

	public EffectThatEmitsActions(object[] actionsToEmit)
	{
		ActionsToEmit = actionsToEmit ?? Array.Empty<object>();
	}
	public override async Task HandleAsync(TestAction action, IDispatcher dispatcher)
	{
		foreach (object actionToEmit in ActionsToEmit)
			await dispatcher.DispatchAsync(actionToEmit);
	}
}
