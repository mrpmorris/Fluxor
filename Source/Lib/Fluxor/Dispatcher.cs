using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Fluxor.Extensions;

namespace Fluxor;

/// <summary>
/// A class that implements <see cref="IDispatcher"/>
/// </summary>
public class Dispatcher : IDispatcher
{
	private readonly SemaphoreSlim SyncRoot = new SemaphoreSlim(1, 1);
	private readonly List<DispatchListener> Listeners = new();
	private readonly Queue<object> QueuedActions = new Queue<object>();

	public async ValueTask AddListenerAsync(DispatchListener listener)
	{
		ArgumentNullException.ThrowIfNull(listener);
		await SyncRoot.ExecuteLockedAsync(async () =>
		{
			Listeners.Add(listener);
			if (Listeners.Count == 1)
				await DequeueActionsAsync();
		});
	}

	public async ValueTask RemoveListenerAsync(DispatchListener listener)
	{
		ArgumentNullException.ThrowIfNull(listener);
		await SyncRoot.WaitAsync();
		try
		{
			Listeners.Remove(listener);
		}
		finally
		{
			SyncRoot.Release();
		}
	}

	/// <see cref="IDispatcher.DispatchAsync(object)"/>
	public async ValueTask DispatchAsync(object action)
	{
		ArgumentNullException.ThrowIfNull(action);

		await SyncRoot.ExecuteLockedAsync(async () =>
		{
			QueuedActions.Enqueue(action);
			await DequeueActionsAsync();
		});
	}

	private async ValueTask DequeueActionsAsync()
	{
		Debug.Assert(SyncRoot.CurrentCount == 0);
		while (QueuedActions.)
		foreach(DispatchListener listener in Listeners)
			await listener.Invoke
	}
}
