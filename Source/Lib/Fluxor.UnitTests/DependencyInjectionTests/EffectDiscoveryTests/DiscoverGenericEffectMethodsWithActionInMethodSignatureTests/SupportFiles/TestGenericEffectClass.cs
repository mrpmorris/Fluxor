using System.Threading.Tasks;

namespace Fluxor.UnitTests.DependencyInjectionTests.EffectDiscoveryTests.DiscoverGenericEffectMethodsWithActionInMethodSignatureTests.SupportFiles;

public class DescendantGenericEffectClass : OpenTestGenericEffectClass<TestAction>
{
	public DescendantGenericEffectClass(InvokeCountService invokeCountService) : base(invokeCountService)
	{
	}
}

public class OpenTestGenericEffectClass<T> 
{
	private readonly InvokeCountService InvokeCountService;

	public OpenTestGenericEffectClass(InvokeCountService invokeCountService)
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
