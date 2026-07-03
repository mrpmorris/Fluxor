using Fluxor.UnitTests.DependencyInjectionTests.EffectDiscoveryTests.DiscoverEffectsWithActionInAttributeTests.SupportFiles;
using System.Threading.Tasks;

namespace Fluxor.UnitTests.DependencyInjectionTests.EffectDiscoveryTests.DiscoverEffectsWithActionInAttributeTests.TypesThatShouldOnlyBeScannedExplicitly;

public class ExplicitlyScannedInstanceTestEffects
{
	[EffectMethod(typeof(TestAction))]
	public async Task Handle(IDispatcher dispatcher)
	{
		await dispatcher.DispatchAsync(new EffectDispatchedAction());
	}
}

public static class ExplicitlyScannedStaticTestEffects
{
	[EffectMethod(typeof(TestAction))]
	public static async Task Handle(IDispatcher dispatcher)
	{
		await dispatcher.DispatchAsync(new EffectDispatchedAction());
	}
}
