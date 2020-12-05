using Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverGenericReducersWithActionInMethodSignatureTests.SupportFiles;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverGenericReducersWithActionInMethodSignatureTests
{
	public class DiscoverGenericReducersWithActionInMethodSignatureTests
	{
		private readonly IServiceProvider ServiceProvider;
		private readonly IDispatcher Dispatcher;
		private readonly IStore Store;
		private readonly IState<TestState<int>> State;

		[Fact]
		public void WhenActionIsDispatched_ThenReducerWithActionInMethodSignatureIsExecuted()
		{
			Assert.Contains(8, State.Value.Items);
			Dispatcher.Dispatch(new RemoveItemAction<int>(8));
			Assert.DoesNotContain(8, State.Value.Items);
		}

		public DiscoverGenericReducersWithActionInMethodSignatureTests()
		{
			var services = new ServiceCollection();
			services.AddFluxor(x => x
				.ScanAssemblies(GetType().Assembly)
				.AddMiddleware<IsolatedTests>());

			ServiceProvider = services.BuildServiceProvider();
			Dispatcher = ServiceProvider.GetRequiredService<IDispatcher>();
			Store = ServiceProvider.GetRequiredService<IStore>();
			State = ServiceProvider.GetRequiredService<IState<TestState<int>>>();

			Store.InitializeAsync().Wait();
		}
	}
}
