using System.Threading.Tasks;

namespace Fluxor.UnitTests.DependencyInjectionTests.EffectDiscoveryTests.DiscoverEffectsWithActionInAttributeTests.SupportFiles;

public class InstanceTestEffects
{
	[EffectMethod(typeof(TestAction))]
	public Task Handle(IDispatcher dispatcher)
	{
		dispatcher.Dispatch(new EffectDispatchedAction());
		return Task.CompletedTask;
	}
}

public static class StaticTestEffects
{
	[EffectMethod(typeof(TestAction))]
	public static Task Handle(IDispatcher dispatcher)
	{
		dispatcher.Dispatch(new EffectDispatchedAction());
		return Task.CompletedTask;
	}
}
