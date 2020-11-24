using Fluxor.DependencyInjection.Microsoft;
using Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverGenericReducerClassesTests.SupportFiles;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverGenericReducerClassesTests
{
	public class DiscoverGenericReducerClassesTests
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

		public DiscoverGenericReducerClassesTests()
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
