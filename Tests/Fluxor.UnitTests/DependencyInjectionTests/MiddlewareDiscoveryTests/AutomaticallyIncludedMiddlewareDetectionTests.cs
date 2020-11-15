using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.UnitTests.DependencyInjectionTests.MiddlewareDiscoveryTests
{
	public class AutomaticallyIncludedMiddlewareDetectionTests
	{
		private readonly IServiceProvider ServiceProvider;
		private readonly IStore Store;

		[Fact]
		public void WhenMiddlewareHasAutomaticallyIncludedMiddlewareAttribute_ThenMiddlewareIsAddedToStoreAutomatically()
		{
			Assert.Single(Store.GetMiddlewares());
			Assert.IsType<AutoMiddleware>(Store.GetMiddlewares().Single());
		}

		[Fact]
		public async Task WhenAutomaticallyIncludedMiddlewareIsAlsoAddedManually_ThenMiddlewareIsOnlyAddedOnce()
		{
			var services = new ServiceCollection();
			services.AddFluxor(x => x
				.ScanAssemblies(GetType().Assembly)
				.AddMiddleware<AutoMiddleware>());

			IServiceProvider serviceProvider = services.BuildServiceProvider();
			IStore store = serviceProvider.GetRequiredService<IStore>();

			await store.InitializeAsync().ConfigureAwait(false);

			Assert.Single(Store.GetMiddlewares());
			Assert.IsType<AutoMiddleware>(Store.GetMiddlewares().Single());
		}

		public AutomaticallyIncludedMiddlewareDetectionTests()
		{
			var services = new ServiceCollection();
			services.AddFluxor(x => x
				.ScanAssemblies(GetType().Assembly));

			ServiceProvider = services.BuildServiceProvider();
			Store = ServiceProvider.GetRequiredService<IStore>();

			Store.InitializeAsync().Wait();
		}

	}
}
