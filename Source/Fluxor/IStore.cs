using Fluxor.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fluxor
{
	/// <summary>
	/// Identifies a store, which is a collection of features. It is recommended that you do not create your
	/// own classes that implement this interface as the <see cref="Store"/> class does this for you.
	/// </summary>
	public interface IStore : IDispatcher, IActionSubscriber
	{
		/// <summary>
		/// This method will register an effect so that it
		/// is executed whenever an action dispatched via the store.
		/// </summary>
		/// <param name="effect">The instance of the effect to call back</param>
		/// <seealso cref="IEffect.HandleAsync(object, IDispatcher)"/>
		void AddEffect(IEffect effect);

		/// <summary>
		/// Adds a feature to the store. Once added, the feature will be notified of all actions dispatched
		/// via the store so that it can keep its state up to date.
		/// </summary>
		/// <param name="feature">The feature to add</param>
		void AddFeature(IFeature feature);

		/// <summary>
		/// Adds a Middleware instance to the store. The Middleware will be notified of various events ocurring
		/// in the store and be able to influence what happens as a result.
		/// </summary>
		/// <param name="middleware">The instance of the Middleware to hook into the store</param>
		void AddMiddleware(IMiddleware middleware);

		/// <summary>
		/// Sometimes Middleware may need to indicate that code currently being executed is running within
		/// some kind of special context.
		/// </summary>
		/// <remarks>
		/// <para>
		/// For example. When the <see cref="ReduxDevTools.ReduxDevToolsMiddleware"/> Middleware sets the state
		/// of all features to a historical state it might cause the current browser URL to change if one of the
		/// historical actions changed the address. The <see cref="Routing.RoutingMiddleware"/> will normally
		/// dispatch a <see cref="Routing.Go"/> action when the address changes, but when the address changes
		/// due to viewing a historical state we do not want to dispatch new actions to record the change.
		/// </para>
		/// <para>
		/// To deal with this scenario the <see cref="ReduxDevTools.ReduxDevToolsMiddleware"/> Middleware ensures
		/// that all Middlewares of the store know we are in a special state in which normal execution should not
		/// occur. The <see cref="Routing.RoutingMiddleware"/> Middleware can detect we are in an "internal change"
		/// and not dispatch a Go action when the browser URL changes.
		/// </para>
		/// </remarks>
		/// <example>
		/// using(Store.BeginInternalMiddlewareChange()) 
		/// {
		///		// Do some stuff that other Middlewares should know was not triggered by a 
		///		// normal user event.
		/// }
		/// </example>
		/// <returns>A disposable that should have Dispose() called on to indicate the internal change is complete</returns>
		/// <see cref="Middleware.IsInsideMiddlewareChange"/>
		/// <seealso cref="ReduxDevTools.ReduxDevToolsMiddleware.OnJumpToState(ReduxDevTools.CallbackObjects.JumpToStateCallback)"/>
		/// <seealso cref="Routing.RoutingMiddleware.LocationChanged(object, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs)"/>
		IDisposable BeginInternalMiddlewareChange();

		/// <summary>
		/// All of the features added to the store, keyed by their unique name.
		/// </summary>
		IReadOnlyDictionary<string, IFeature> Features { get; }

		/// <summary>
		/// This method should be executed when the store is first ready to be initialized.
		/// It will, in turn, initialise any middleware. This method can safely be executed
		/// more than once.
		/// </summary>
		/// <returns>Task</returns>
		Task InitializeAsync();

		/// <summary>
		/// Await this task if you need to asynchronously wait for the store to initialise
		/// </summary>
		/// <see cref="InitializeAsync()"/>
		Task Initialized { get; }

		/// <summary>
		/// Returns a list of registered middleware
		/// </summary>
		/// <returns>Middleware instances currently registered</returns>
		IEnumerable<IMiddleware> GetMiddlewares();
		
		/// <summary>
		/// Executed when an exception is not handled
		/// </summary>
		event EventHandler<Exceptions.UnhandledExceptionEventArgs> UnhandledException;
	}
}
