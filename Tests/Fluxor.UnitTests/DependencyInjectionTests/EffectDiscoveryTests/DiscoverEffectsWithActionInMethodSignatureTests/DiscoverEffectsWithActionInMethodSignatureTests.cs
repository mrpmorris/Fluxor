using Fluxor.UnitTests.DependencyInjectionTests.EffectDiscoveryTests.DiscoverEffectsWithActionInMethodSignatureTests.SupportFiles;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace Fluxor.UnitTests.DependencyInjectionTests.EffectDiscoveryTests.DiscoverEffectsWithActionInMethodSignatureTests
{
	public class DiscoverEffectsWithActionInMethodSignatureTests
	{
		private readonly IServiceProvider ServiceProvider;
		private readonly IStore Store;
		private readonly InvokeCountService InvokeCountService;

		[Fact]
		public void WhenActionIsDispatched_ThenEffectWithActionInMethodSignatureIsExecuted()
		{
			Assert.Equal(0, InvokeCountService.GetCount());
			Store.Dispatch(new TestAction());
			Assert.Equal(1, InvokeCountService.GetCount());
		}

		public DiscoverEffectsWithActionInMethodSignatureTests()
		{
			InvokeCountService = new InvokeCountService();

			var services = new ServiceCollection();
			services.AddScoped(_ => InvokeCountService);
			services.AddFluxor(x => x
				.ScanAssemblies(GetType().Assembly)
				.AddMiddleware<IsolatedTests>());

			ServiceProvider = services.BuildServiceProvider();
			Store = ServiceProvider.GetRequiredService<IStore>();

			Store.InitializeAsync().Wait();
		}

	}
}
