using System.Threading.Tasks;

namespace Fluxor.UnitTests.DependencyInjectionTests.EffectDiscoveryTests.DiscoverEffectsWithActionInMethodSignatureTests.SupportFiles
{
	public class TestEffects
	{
		private readonly InvokeCountService InvokeCountService;

		public TestEffects(InvokeCountService invokeCountService)
		{
			InvokeCountService = invokeCountService;
		}

		[EffectMethod]
		public Task Handle(TestAction action, IDispatcher dispatcher)
		{
			InvokeCountService.IncrementCount();
			return Task.CompletedTask;
		}
	}
}
