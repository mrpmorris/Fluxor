using System;

namespace Fluxor.Exceptions
{
	/// <summary>
	/// Args that describe an exception in an effect that was not handled
	/// </summary>
	public class UnhandledExceptionEventArgs : EventArgs
	{
		/// <summary>
		/// True if the application handled the exception, otherwise the
		/// exception should be rethrown by the UI
		/// </summary>
		public bool WasHandled { get; private set; }
		/// <summary>
		/// The unhandled exception
		/// </summary>
		public Exception Exception { get; }

		/// <summary>
		/// Creates a new instance
		/// </summary>
		/// <param name="exception">The exception that was unhandled</param>
		public UnhandledExceptionEventArgs(Exception exception)
		{
			Exception = exception ?? throw new ArgumentNullException(nameof(exception));
		}

		/// <summary>
		/// Sets <see cref="WasHandled"/> to true, which informs the UI not to
		/// rethrow the exception
		/// </summary>
		public void Handled()
		{
			WasHandled = true;
		}
	}
}
