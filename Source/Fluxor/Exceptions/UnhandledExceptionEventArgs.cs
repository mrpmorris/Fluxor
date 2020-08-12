using System;

namespace Fluxor.Exceptions
{
	public class UnhandledExceptionEventArgs : EventArgs
	{
		public bool WasHandled { get; private set; }
		public Exception Exception { get; }

		public UnhandledExceptionEventArgs(Exception exception)
		{
			Exception = exception ?? throw new ArgumentNullException(nameof(exception));
		}

		public void Handled()
		{
			WasHandled = true;
		}
	}
}
