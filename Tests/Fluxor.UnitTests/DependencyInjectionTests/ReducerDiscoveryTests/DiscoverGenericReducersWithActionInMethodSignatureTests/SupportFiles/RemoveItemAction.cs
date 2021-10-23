namespace Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverGenericReducersWithActionInMethodSignatureTests.SupportFiles
{
	public class IncrementItemAction<T>
	{
		public readonly T Item;

		public IncrementItemAction(T item)
		{
			Item = item;
		}
	}
}
