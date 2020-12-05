using Fluxor.UnitTests.DependencyInjectionTests.EffectDiscoveryTests.DiscoverGenericEffectMethodsWithActionInMethodSignatureTests.SupportFiles;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace Fluxor.UnitTests.DependencyInjectionTests.EffectDiscoveryTests.DiscoverGenericEffectMethodsWithActionInMethodSignatureTests
{
	public class DiscoverGenericEffectMethodsWithActionInMethodSignatureTests
	{
		private readonly IServiceProvider ServiceProvider;
		private readonly IDispatcher Dispatcher;
		private readonly IStore Store;
		private readonly InvokeCountService InvokeCountService;

		[Fact]
		public void WhenActionIsDispatched_ThenGenericEffectClassIsExecuted()
		{
			Assert.Equal(0, InvokeCountService.GetCount());
			Dispatcher.Dispatch(new TestAction());
			Assert.Equal(1, InvokeCountService.GetCount());
		}

		public DiscoverGenericEffectMethodsWithActionInMethodSignatureTests()
		{
			InvokeCountService = new InvokeCountService();
			var services = new ServiceCollection();
			services.AddScoped(_ => InvokeCountService);
			services.AddFluxor(x => x
				.ScanAssemblies(GetType().Assembly)
				.AddMiddleware<IsolatedTests>());

			ServiceProvider = services.BuildServiceProvider();
			Dispatcher = ServiceProvider.GetRequiredService<IDispatcher>();
			Store = ServiceProvider.GetRequiredService<IStore>();

			Store.InitializeAsync().Wait();
		}

	}
}
