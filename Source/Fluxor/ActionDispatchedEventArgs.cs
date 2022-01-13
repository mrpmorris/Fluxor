using System;

namespace Fluxor
{
	public class ActionDispatchedEventArgs : EventArgs
	{
		public object Action { get; private set; }

		public ActionDispatchedEventArgs(object action)
		{
			Action = action ?? throw new ArgumentNullException(nameof(action));
		}
	}
}
