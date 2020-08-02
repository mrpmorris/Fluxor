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
		private readonly string Id;
		private readonly Action Action;
		private bool IsDisposed;
		private bool WasCreated;

		/// <summary>
		/// Creates an instance of the class
		/// </summary>
		/// <param name="id">
		///		An Id that is included in the message of exceptions that are thrown, this is useful
		///		for helping to identify the source that created the instance that threw the exception.
		/// </param>
		/// <param name="action">The action to execute when the instance is disposed</param>
		public DisposableCallback(string id, Action action)
		{
			if (string.IsNullOrWhiteSpace(id))
				throw new ArgumentNullException(nameof(id));
			if (action == null)
				 throw new ArgumentNullException(nameof(action));

			Id = id;
			Action = action;
			WasCreated = true;
		}

		/// <summary>
		/// Executes the action when disposed
		/// </summary>
		public void Dispose()
		{
			if (IsDisposed)
				throw new ObjectDisposedException(
					nameof(DisposableCallback),
					$"Attempt to call {nameof(Dispose)} twice on {nameof(DisposableCallback)} with Id \"{Id}\".");

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
			if (!IsDisposed && WasCreated)
				throw new InvalidOperationException($"{nameof(DisposableCallback)} with Id \"{Id}\" was not disposed. " +
					$"See https://github.com/mrpmorris/Fluxor/tree/master/Docs/disposable-callback-not-disposed.md for more details");
		}
	}
}
