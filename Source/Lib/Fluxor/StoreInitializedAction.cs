namespace Fluxor;

/// <summary>
/// Dispatched by the store once it has been initialised. This is usually the first
/// action dispatched, although it might not be because Middlewares may dispatch
/// actions earlier on.
/// </summary>
public class StoreInitializedAction
{
	public bool WasPersisted => Store.WasPersisted;

	private readonly IStore Store;
	internal StoreInitializedAction(IStore store)
	{
		Store = store;
	}
}
