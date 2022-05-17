using System.Threading.Tasks;

namespace Fluxor.UnitTests.DependencyInjectionTests.EffectDiscoveryTests.DiscoverGenericEffectClassesTests.SupportFiles
{
	public class DescendantGenericEffectClass : OpenGenericEffectClass<TestAction>
	{
		public DescendantGenericEffectClass(InvokeCountService invokeCountService) : base(invokeCountService)
		{
		}
	}

	public class OpenGenericEffectClass<T> : Effect<T>
	{
		private readonly InvokeCountService InvokeCountService;

		public OpenGenericEffectClass(InvokeCountService invokeCountService)
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
