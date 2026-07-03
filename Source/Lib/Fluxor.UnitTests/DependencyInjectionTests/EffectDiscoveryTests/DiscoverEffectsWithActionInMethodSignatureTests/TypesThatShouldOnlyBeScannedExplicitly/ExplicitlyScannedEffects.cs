using Fluxor.UnitTests.DependencyInjectionTests.EffectDiscoveryTests.DiscoverEffectsWithActionInMethodSignatureTests.SupportFiles;
using System.Threading.Tasks;

namespace Fluxor.UnitTests.DependencyInjectionTests.EffectDiscoveryTests.DiscoverEffectsWithActionInMethodSignatureTests.TypesThatShouldOnlyBeScannedExplicitly;

public class ExplicitlyScannedInstanceTestEffects
{
	[EffectMethod]
	public async Task Handle(TestAction action, IDispatcher dispatcher)
	{
		await dispatcher.DispatchAsync(new EffectDispatchedAction());
	}
}

public static class ExplicitlyScannedStaticTestEffects
{
	[EffectMethod]
	public static async Task Handle(TestAction action, IDispatcher dispatcher)
	{
		await dispatcher.DispatchAsync(new EffectDispatchedAction());
	}
}
