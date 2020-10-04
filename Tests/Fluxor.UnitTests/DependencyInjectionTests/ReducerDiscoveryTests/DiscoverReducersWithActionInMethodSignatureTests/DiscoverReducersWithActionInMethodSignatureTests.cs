using Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverReducersWithActionInMethodSignatureTests.SupportFiles;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverReducersWithActionInMethodSignatureTests
{
	public class DiscoverReducersWithActionInMethodSignatureTests
	{
		private readonly IServiceProvider ServiceProvider;
		private readonly IStore Store;
		private readonly IState<TestState> State;

		[Fact]
		public void WhenActionIsDispatched_ThenReducerWithActionInMethodSignatureIsExecuted()
		{
			Assert.False(State.Value.ReducerWasExecuted);
			Store.Dispatch(new TestAction());
			Assert.True(State.Value.ReducerWasExecuted);
		}

		public DiscoverReducersWithActionInMethodSignatureTests()
		{
			var services = new ServiceCollection();
			services.AddFluxor(x => x
				.ScanAssemblies(GetType().Assembly)
				.AddMiddleware<IsolatedTests>());

			ServiceProvider = services.BuildServiceProvider();
			Store = ServiceProvider.GetRequiredService<IStore>();
			State = ServiceProvider.GetRequiredService<IState<TestState>>();

			Store.InitializeAsync().Wait();
		}
	}
}
