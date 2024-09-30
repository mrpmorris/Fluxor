namespace Fluxor.UnitTests.DependencyInjectionTests.EffectDiscoveryTests.DiscoverGenericEffectMethodsWithActionInMethodSignatureTests.SupportFiles;

public class InvokeCountService
{
	private int Count;

	public int GetCount() => Count;
	public void IncrementCount()
	{
		Count++;
	}
}
