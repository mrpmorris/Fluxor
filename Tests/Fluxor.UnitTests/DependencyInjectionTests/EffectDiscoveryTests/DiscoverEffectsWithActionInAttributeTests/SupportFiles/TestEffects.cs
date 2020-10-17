using System.Threading.Tasks;

namespace Fluxor.UnitTests.DependencyInjectionTests.EffectDiscoveryTests.DiscoverEffectsWithActionInAttributeTests.SupportFiles
{
	public class TestEffects
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
		public static Task HandleTestAction(IDispatcher dispatcher)
		{
			dispatcher.Dispatch(new EffectDispatchedAction());
			return Task.CompletedTask;
		}
	}
}
