using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.UnitTests.DependencyInjectionTests.MiddlewareDiscoveryTests
{
	public class AutomaticallyIncludedMiddlewareDetectionTests
	{
		private readonly IServiceProvider ServiceProvider;
		private readonly IDispatcher Dispatcher;
		private readonly IStore Store;

		[Fact]
		public void WhenMiddlewareHasAutomaticallyIncludedMiddlewareAttribute_ThenMiddlewareIsAddedToStoreAutomatically()
		{
			Assert.Single(Store.GetMiddlewares());
			Assert.IsType<AutoMiddleware>(Store.GetMiddlewares().Single());
		}

		[Fact]
		public void WhenMiddlewareHasAutomaticallyIncludedMiddlewareAttribute_ThenClassesInSameNamespaceAreAlsoIncluded()
		{
			Effects.WasExecuted = false;
			Dispatcher.Dispatch("Hello");
			Assert.True(Effects.WasExecuted);
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
			Dispatcher = ServiceProvider.GetRequiredService<IDispatcher>();
			Store = ServiceProvider.GetRequiredService<IStore>();

			Store.InitializeAsync().Wait();
		}
	}
}
