using Fluxor.UnitTests.DependencyInjectionTests.EffectDiscoveryTests.DiscoverEffectsWithActionInMethodSignatureTests.SupportFiles;
using System.Threading.Tasks;

namespace Fluxor.UnitTests.DependencyInjectionTests.EffectDiscoveryTests.DiscoverEffectsWithActionInMethodSignatureTests.TypesThatShouldOnlyBeScannedExplicitly
{
	public class InstanceTestEffects
	{
		[EffectMethod]
		public Task Handle(TestAction action, IDispatcher dispatcher)
		{
			dispatcher.Dispatch(new EffectDispatchedAction());
			return Task.CompletedTask;
		}
	}

	public static class StaticTestEffects
	{
		[EffectMethod]
		public static Task Handle(TestAction action, IDispatcher dispatcher)
		{
			dispatcher.Dispatch(new EffectDispatchedAction());
			return Task.CompletedTask;
		}
	}
}
