using System;

namespace Fluxor.Exceptions
{
	/// <summary>
	/// Base exception class for Fluxor
	/// </summary>
	public abstract class FluxorException : Exception
	{
		/// <summary>
		/// Creates a new instance of the exception
		/// </summary>
		/// <param name="message">The message that describes the error</param>
		public FluxorException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Creates a new instance of the exception
		/// </summary>
		/// <param name="message">The message that describes the error</param>
		/// <param name="innerException">The exception that is the cause of the current exception, or a null reference</param>
		public FluxorException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
