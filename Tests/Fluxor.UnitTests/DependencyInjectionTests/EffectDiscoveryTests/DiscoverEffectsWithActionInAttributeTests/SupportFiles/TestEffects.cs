using System.Threading.Tasks;

namespace Fluxor.UnitTests.DependencyInjectionTests.EffectDiscoveryTests.DiscoverEffectsWithActionInAttributeTests.SupportFiles
{
	public class TestEffects
	{
		private readonly InvokeCountService InvokeCountService;

		public TestEffects(InvokeCountService invokeCountService)
		{
			InvokeCountService = invokeCountService;
		}

		[EffectMethod(typeof(TestAction))]
		public Task Handle(IDispatcher dispatcher)
		{
			InvokeCountService.IncrementCount();
			return Task.CompletedTask;
		}
	}
}
