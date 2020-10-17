namespace Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverGenericReducersWithActionInMethodSignatureTests.SupportFiles
{
	public class RemoveItemAction<T>
	{
		public readonly T Item;

		public RemoveItemAction(T item)
		{
			Item = item;
		}
	}
}
