using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fluxor
{
	/// <see cref="IMiddleware"/>
	public abstract class Middleware : IMiddleware
	{
		/// <summary>
		/// Number of times <see cref="IMiddleware.BeginInternalMiddlewareChange"/> has been called
		/// </summary>
		protected volatile int BeginMiddlewareChangeCount;

		/// <summary>
		/// True if <see cref="BeginMiddlewareChangeCount"/> is greater than zero
		/// </summary>
		protected bool IsInsideMiddlewareChange => BeginMiddlewareChangeCount > 0;

		/// <see cref="IMiddleware.InitializeAsync(IStore)"/>
		public virtual Task InitializeAsync(IStore store) => Task.CompletedTask;

		/// <see cref="IMiddleware.AfterInitializeAllMiddlewares"/>
		public virtual void AfterInitializeAllMiddlewares() { }

		/// <see cref="IMiddleware.MayDispatchAction(object)"/>
		public virtual bool MayDispatchAction(object action) => true;

		/// <see cref="IMiddleware.BeforeDispatch(object)"/>
		public virtual void BeforeDispatch(object action) { }

		/// <see cref="IMiddleware.AfterDispatch(object)"/>
		public virtual void AfterDispatch(object action) { }

		/// <summary>
		/// Executed when <see cref="BeginMiddlewareChangeCount"/> becomes zero
		/// </summary>
		protected virtual void OnInternalMiddlewareChangeEnding() { }

		IDisposable IMiddleware.BeginInternalMiddlewareChange()
		{
			Interlocked.Increment(ref BeginMiddlewareChangeCount);
			return new DisposableCallback(
				id: $"{nameof(Middleware)} {nameof(IMiddleware)}.{nameof(IMiddleware.BeginInternalMiddlewareChange)}",
				() =>
					{
						if (BeginMiddlewareChangeCount == 1)
							OnInternalMiddlewareChangeEnding();
						Interlocked.Decrement(ref BeginMiddlewareChangeCount);
					});
		}
	}
}
