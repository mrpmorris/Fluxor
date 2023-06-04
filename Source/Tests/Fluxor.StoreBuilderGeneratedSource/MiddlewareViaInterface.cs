namespace Fluxor.StoreBuilderGeneratedSource;

internal class MiddlewareViaInterface : IMiddleware
{
	public void AfterDispatch(object action)
	{
	}

	public void AfterInitializeAllMiddlewares()
	{
	}

	public void BeforeDispatch(object action)
	{
	}

	public IDisposable BeginInternalMiddlewareChange() => null;

	public Task InitializeAsync(IDispatcher dispatcher, IStore store) => Task.CompletedTask;

	public bool MayDispatchAction(object action) => true;
}
