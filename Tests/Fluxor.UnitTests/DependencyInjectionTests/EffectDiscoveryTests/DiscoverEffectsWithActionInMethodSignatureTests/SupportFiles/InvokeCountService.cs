namespace Fluxor.UnitTests.DependencyInjectionTests.EffectDiscoveryTests.DiscoverEffectsWithActionInMethodSignatureTests.SupportFiles
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
