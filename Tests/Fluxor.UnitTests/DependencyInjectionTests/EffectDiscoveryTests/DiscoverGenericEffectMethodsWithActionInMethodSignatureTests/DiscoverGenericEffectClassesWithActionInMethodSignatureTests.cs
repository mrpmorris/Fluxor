using Fluxor.UnitTests.DependencyInjectionTests.EffectDiscoveryTests.DiscoverGenericEffectMethodsWithActionInMethodSignatureTests.SupportFiles;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace Fluxor.UnitTests.DependencyInjectionTests.EffectDiscoveryTests.DiscoverGenericEffectMethodsWithActionInMethodSignatureTests
{
	public class DiscoverGenericEffectMethodsWithActionInMethodSignatureTests
	{
		private readonly IServiceProvider ServiceProvider;
		private readonly IStore Store;
		private readonly InvokeCountService InvokeCountService;

		[Fact]
		public void WhenActionIsDispatched_ThenGenericEffectClassIsExecuted()
		{
			Assert.Equal(0, InvokeCountService.GetCount());
			Store.Dispatch(new TestAction());
			// 2 Effects
			// 1 descendant of the generic
			// + 1 explicitly specified closed generic
			Assert.Equal(2, InvokeCountService.GetCount());
		}

		public DiscoverGenericEffectMethodsWithActionInMethodSignatureTests()
		{
			InvokeCountService = new InvokeCountService();
			var services = new ServiceCollection();
			services.AddScoped(_ => InvokeCountService);
			services.AddFluxor(x => x
				.ScanAssemblies(GetType().Assembly)
				.ScanTypes(typeof(AbstractTestGenericEffectClass<TestAction>))
				.AddMiddleware<IsolatedTests>());

			ServiceProvider = services.BuildServiceProvider();
			Store = ServiceProvider.GetRequiredService<IStore>();

			Store.InitializeAsync().Wait();
		}

	}
}
