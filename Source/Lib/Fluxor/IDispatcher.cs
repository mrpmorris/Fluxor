using System;

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
	void Dispatch(object action);

	/// <summary>
	/// An event that is triggered whenever <see cref="Dispatch(object)"/> is executed.
	/// </summary>
	event EventHandler<ActionDispatchedEventArgs> ActionDispatched;
}
