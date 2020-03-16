using Fluxor.Exceptions;
using System;

namespace Fluxor.Exceptions
{
	/// <summary>
	/// An exception that is thrown when the Store fails to initialize
	/// </summary>
	public class StoreInitializationException : FluxorException
	{
		/// <summary>
		/// Creates a new instance of the exception
		/// </summary>
		/// <param name="message">The message that describes the error</param>
		public StoreInitializationException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Creates a new instance of the exception
		/// </summary>
		/// <param name="message">The message that describes the error</param>
		/// <param name="innerException">The exception that is the cause of the current exception, or a null reference</param>
		public StoreInitializationException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
