using System;

namespace Fluxor
{
	/// <summary>
	/// This class can be used to execute a custom piece of code when IDisposable is
	/// called.
	/// </summary>
	/// <seealso cref="IStore.BeginInternalMiddlewareChange()"/>
	public sealed class DisposableCallback : IDisposable
	{
		private readonly Action Action;
		private bool IsDisposed;

		/// <summary>
		/// Creates an instance of the class
		/// </summary>
		/// <param name="action">The action to execute when the instance is disposed</param>
		public DisposableCallback(Action action)
		{
			Action = action ?? throw new ArgumentNullException(nameof(action));
		}

		/// <summary>
		/// Executes the action when disposed
		/// </summary>
		public void Dispose()
		{
			if (IsDisposed)
				throw new ObjectDisposedException(nameof(DisposableCallback));

			IsDisposed = true;
			GC.SuppressFinalize(this);
			Action();
		}

		/// <summary>
		/// Throws an exception if this object is collected without being disposed
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if the object is collected without being disposed</exception>
		~DisposableCallback()
		{
			if (!IsDisposed)
				throw new InvalidOperationException("DisposableCallback was not disposed");
		}
	}
}
