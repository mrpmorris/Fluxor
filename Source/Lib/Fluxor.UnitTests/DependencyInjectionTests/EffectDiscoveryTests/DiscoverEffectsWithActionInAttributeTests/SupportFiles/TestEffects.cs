using System.Threading.Tasks;

namespace Fluxor.UnitTests.DependencyInjectionTests.EffectDiscoveryTests.DiscoverEffectsWithActionInAttributeTests.SupportFiles;

public class InstanceTestEffects
{
	[EffectMethod(typeof(TestAction))]
	public async Task Handle(IDispatcher dispatcher)
	{
		await dispatcher.DispatchAsync(new EffectDispatchedAction());
	}
}

public static class StaticTestEffects
{
	[EffectMethod(typeof(TestAction))]
	public static async Task Handle(IDispatcher dispatcher)
	{
		await dispatcher.DispatchAsync(new EffectDispatchedAction());
	}
}
