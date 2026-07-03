using System.Threading.Tasks;

namespace Fluxor.UnitTests.DependencyInjectionTests.EffectDiscoveryTests.DiscoverEffectsWithActionInMethodSignatureTests.SupportFiles;

public class InstanceTestEffects
{
	[EffectMethod]
	public async Task Handle(TestAction action, IDispatcher dispatcher)
	{
		await dispatcher.DispatchAsync(new EffectDispatchedAction());
	}
}

public static class StaticTestEffects
{
	[EffectMethod]
	public static async Task Handle(TestAction action, IDispatcher dispatcher)
	{
		await dispatcher.DispatchAsync(new EffectDispatchedAction());
	}
}
