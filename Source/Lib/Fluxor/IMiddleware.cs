using System;
using System.Threading.Tasks;

namespace Fluxor;

/// <summary>
/// An interface for implementing middleware
/// </summary>
public interface IMiddleware
{
	/// <summary>
	/// Called exactly once by the store when the store initialises, or when
	/// the middleware is added to the store (if the store has already been initialised)
	/// </summary>
	/// <param name="dispatcher">
	/// A reference to the dispatcher. Never await <see cref="IDispatcher.DispatchAsync(object)"/>
	/// from within this method; dispatched actions cannot complete until store activation has
	/// finished, and activation is awaiting this method, so awaiting would deadlock. Discard
	/// the task instead (<c>_ = dispatcher.DispatchAsync(action);</c>).
	/// </param>
	/// <param name="store">A reference to the store</param>
	Task InitializeAsync(IDispatcher dispatcher, IStore store);

	/// <summary>
	/// Called exactly once by the store after <see cref="InitializeAsync(IStore)"/> has been
	/// called on all registered Middlewares
	/// </summary>
	void AfterInitializeAllMiddlewares();

	/// <summary>
	/// Called before each action dispatched
	/// </summary>
	/// <param name="action">The action to be dispatched</param>
	/// <returns>True if the action may proceed, False if it should be prevented</returns>
	bool MayDispatchAction(object action);

	/// <summary>
	/// Called before each action dispatched
	/// </summary>
	/// <param name="action">The action being dispatched</param>
	void BeforeDispatch(object action);

	/// <summary>
	/// Called after each action dispatched
	/// </summary>
	/// <param name="action">The action that has just been dispatched</param>
	void AfterDispatch(object action);

	/// <summary>
	/// This should only be called via <see cref="IStore.BeginInternalMiddlewareChange"/>.
	/// </summary>
	/// <returns>An IDisposable that should be executed when the internal change ends</returns>
	/// <seealso cref="DisposableCallback"/>
	IDisposable BeginInternalMiddlewareChange();
}
