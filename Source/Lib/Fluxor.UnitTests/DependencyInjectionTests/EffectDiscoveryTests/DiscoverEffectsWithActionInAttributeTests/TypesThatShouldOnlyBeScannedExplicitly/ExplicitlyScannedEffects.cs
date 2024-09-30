using Fluxor.UnitTests.DependencyInjectionTests.EffectDiscoveryTests.DiscoverEffectsWithActionInAttributeTests.SupportFiles;
using System.Threading.Tasks;

namespace Fluxor.UnitTests.DependencyInjectionTests.EffectDiscoveryTests.DiscoverEffectsWithActionInAttributeTests.TypesThatShouldOnlyBeScannedExplicitly;

public class ExplicitlyScannedInstanceTestEffects
{
	[EffectMethod(typeof(TestAction))]
	public Task Handle(IDispatcher dispatcher)
	{
		dispatcher.Dispatch(new EffectDispatchedAction());
		return Task.CompletedTask;
	}
}

public static class ExplicitlyScannedStaticTestEffects
{
	[EffectMethod(typeof(TestAction))]
	public static Task Handle(IDispatcher dispatcher)
	{
		dispatcher.Dispatch(new EffectDispatchedAction());
		return Task.CompletedTask;
	}
}
