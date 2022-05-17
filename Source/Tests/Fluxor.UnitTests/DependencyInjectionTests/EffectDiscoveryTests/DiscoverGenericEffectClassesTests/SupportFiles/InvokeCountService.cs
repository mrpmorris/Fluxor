namespace Fluxor.UnitTests.DependencyInjectionTests.EffectDiscoveryTests.DiscoverGenericEffectClassesTests.SupportFiles
{
	public class InvokeCountService
	{
		private int Count;

		public int GetCount() => Count;
		public void IncrementCount()
		{
			Count++;
		}
	}
}
