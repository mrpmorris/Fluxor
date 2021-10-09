using System.Threading.Tasks;

namespace Fluxor.UnitTests.DependencyInjectionTests.EffectDiscoveryTests.DiscoverGenericEffectMethodsWithActionInMethodSignatureTests.SupportFiles
{
	public class TestGenericEffectClass : AbstractTestGenericEffectClass<TestAction>
	{
		public TestGenericEffectClass(InvokeCountService invokeCountService) : base(invokeCountService)
		{
		}
	}

	public class AbstractTestGenericEffectClass<T> 
	{
		private readonly InvokeCountService InvokeCountService;

		public AbstractTestGenericEffectClass(InvokeCountService invokeCountService)
		{
			InvokeCountService = invokeCountService;
		}

		[EffectMethod]
		public Task HandleTheActionAsync(T action, IDispatcher dispatcher)
		{
			InvokeCountService.IncrementCount();
			return Task.CompletedTask;
		}
	}
}
