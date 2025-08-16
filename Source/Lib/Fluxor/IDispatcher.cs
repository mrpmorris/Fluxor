using System.Threading.Tasks;

namespace Fluxor;

public delegate ValueTask DispatchListener(object action);

/// <summary>
/// Interface that blazor components/pages should use to dispatch actions
/// through the store
/// </summary>
public interface IDispatcher
{
	ValueTask AddListenerAsync(DispatchListener listener);
	ValueTask RemoveListenerAsync(DispatchListener listener);

	/// <summary>
	/// Dispatches an action to all features added to the store and ensures all effects with a regstered
	/// interest in the action type are notified.
	/// </summary>
	/// <param name="action">The action to dispatch to all features</param>
	ValueTask DispatchAsync(object action);
}
