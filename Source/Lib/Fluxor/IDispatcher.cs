using System;
using System.Threading.Tasks;

namespace Fluxor;

/// <summary>
/// Interface that blazor components/pages should use to dispatch actions
/// through the store
/// </summary>
public interface IDispatcher
{
	/// <summary>
	/// Dispatches an action to all features added to the store and ensures all effects with a regstered
	/// interest in the action type are notified.
	/// </summary>
	/// <param name="action">The action to dispatch to all features</param>
	/// <returns>
	/// A task that completes once all middleware hooks, reducers, action subscribers, and all
	/// effects triggered by the action have completed. The task faults if a middleware hook,
	/// reducer, or action subscriber throws, or if any effect throws (all effect exceptions
	/// are aggregated). If no store is subscribed to <see cref="ActionDispatched"/> the action
	/// remains queued and the task remains pending until a store subscribes.
	/// </returns>
	Task DispatchAsync(object action);

	/// <summary>
	/// An event that is triggered whenever <see cref="DispatchAsync(object)"/> is executed.
	/// </summary>
	event EventHandler<ActionDispatchedEventArgs> ActionDispatched;
}
