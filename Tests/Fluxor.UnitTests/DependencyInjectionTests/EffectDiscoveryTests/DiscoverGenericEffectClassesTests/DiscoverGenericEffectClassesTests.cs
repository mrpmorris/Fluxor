using Fluxor.UnitTests.DependencyInjectionTests.EffectDiscoveryTests.DiscoverGenericEffectClassesTests.SupportFiles;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace Fluxor.UnitTests.DependencyInjectionTests.EffectDiscoveryTests.DiscoverGenericEffectClassesTests
{
	public class DiscoverGenericEffectClassesTests
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
			// 2 Effects
			// 1 assembly scanned (generic descendant)
			// + 1 type scanned (closed generic)
			Assert.Equal(2, InvokeCountService.GetCount());
		}

		public DiscoverGenericEffectClassesTests()
		{
			InvokeCountService = new InvokeCountService();
			var services = new ServiceCollection();
			services.AddScoped(_ => InvokeCountService);
			services.AddFluxor(x => x
				.ScanAssemblies(GetType().Assembly)
				.ScanTypes(typeof(OpenGenericEffectClass<TestAction>))
				.AddMiddleware<IsolatedTests>());

			ServiceProvider = services.BuildServiceProvider();
			Dispatcher = ServiceProvider.GetService<IDispatcher>();
			Store = ServiceProvider.GetRequiredService<IStore>();

			Store.InitializeAsync().Wait();
		}

	}
}
