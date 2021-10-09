using System.Threading.Tasks;

namespace Fluxor.UnitTests.DependencyInjectionTests.EffectDiscoveryTests.DiscoverGenericEffectClassesTests.SupportFiles
{
	public class TestGenericEffectClass : AbstractTestGenericEffectClass<TestAction>
	{
		public TestGenericEffectClass(InvokeCountService invokeCountService) : base(invokeCountService)
		{
		}
	}

	public class AbstractTestGenericEffectClass<T> : Effect<T>
	{
		private readonly InvokeCountService InvokeCountService;

		public AbstractTestGenericEffectClass(InvokeCountService invokeCountService)
		{
			InvokeCountService = invokeCountService;
		}

		public override Task HandleAsync(T action, IDispatcher dispatcher)
		{
			InvokeCountService.IncrementCount();
			return Task.CompletedTask;
		}
	}
}
