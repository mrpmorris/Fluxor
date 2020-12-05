using System;

namespace Fluxor
{
	/// <summary>
	/// Event arguments passed by the <see cref="IDispatcher.ActionDispatching"/> event
	/// </summary>
	public class ActionDispatchingEventArgs : EventArgs
	{
		/// <summary>
		/// The object that was dispatched using <see cref="IDispatcher.Dispatch(object)"/>
		/// </summary>
		public object Action { get; }

		/// <summary>
		/// Creates a new instance of the event args
		/// </summary>
		/// <param name="action">The action dispatched using <see cref="IDispatcher.Dispatch(object)"/></param>
		public ActionDispatchingEventArgs(object action)
		{
			Action = action;
		}
	}
}
