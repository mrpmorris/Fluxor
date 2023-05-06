using System;

namespace Fluxor.StoreBuilderSourceGenerator.Helpers;

internal sealed class DisposableAction : IDisposable
{
	private readonly Action Action;

	public DisposableAction(Action action)
	{
		Action = action;
	}

	void IDisposable.Dispose()
	{
		Action();
	}
}
