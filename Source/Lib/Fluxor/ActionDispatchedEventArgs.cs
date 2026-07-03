using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fluxor;

public class ActionDispatchedEventArgs : EventArgs
{
	public object Action { get; private set; }

	// RunContinuationsAsynchronously ensures continuations of awaiting callers never
	// execute inline inside the store's dequeue loop or while its locks are held.
	internal TaskCompletionSource CompletionSource { get; } =
		new(TaskCreationOptions.RunContinuationsAsynchronously);

	internal Task Completion => CompletionSource.Task;
	internal void Complete() => CompletionSource.TrySetResult();
	internal void Fail(Exception e) => CompletionSource.TrySetException(e);
	internal void Fail(IEnumerable<Exception> e) => CompletionSource.TrySetException(e);

	public ActionDispatchedEventArgs(object action)
	{
		Action = action ?? throw new ArgumentNullException(nameof(action));
	}
}
