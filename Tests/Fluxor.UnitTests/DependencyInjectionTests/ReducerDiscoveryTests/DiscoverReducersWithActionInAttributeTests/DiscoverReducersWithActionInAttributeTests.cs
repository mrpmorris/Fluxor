using Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverReducersWithActionInAttributeTests.SupportFiles;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverReducersWithActionInAttributeTests
{
	public class DiscoverReducersWithActionInAttributeTests
	{
		private readonly IServiceProvider ServiceProvider;
		private readonly IDispatcher Dispatcher;
		private readonly IStore Store;
		private readonly IState<TestState> State;

		[Fact]
		public void WhenActionIsDispatched_ThenReducerWithActionInMethodSignatureIsExecuted()
		{
			Assert.False(State.Value.ReducerWasExecuted);
			Dispatcher.Dispatch(new TestAction());
			Assert.True(State.Value.ReducerWasExecuted);
		}

		public DiscoverReducersWithActionInAttributeTests()
		{
			var services = new ServiceCollection();
			services.AddFluxor(x => x
				.ScanAssemblies(GetType().Assembly)
				.AddMiddleware<IsolatedTests>());

			ServiceProvider = services.BuildServiceProvider();
			Dispatcher = ServiceProvider.GetRequiredService<IDispatcher>();
			Store = ServiceProvider.GetRequiredService<IStore>();
			State = ServiceProvider.GetRequiredService<IState<TestState>>();

			Store.InitializeAsync().Wait();
		}
	}
}
