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
			Assert.Equal(0, State.Value.Count);
			Store.Dispatch(new TestAction());

			// 2 Reducers
			// 1 descendant of the generic
			// + 1 explicitly specified closed generic
			Assert.Equal(2, State.Value.Count);
		}

		public DiscoverGenericReducerClassesTests()
		{
			var services = new ServiceCollection();
			services.AddFluxor(x => x
				.ScanAssemblies(GetType().Assembly)
				.ScanTypes(typeof(OpenGenericReducerClass<TestAction>))
				.AddMiddleware<IsolatedTests>());

			ServiceProvider = services.BuildServiceProvider();
			Store = ServiceProvider.GetRequiredService<IStore>();
			State = ServiceProvider.GetRequiredService<IState<TestState>>();

			Store.InitializeAsync().Wait();
		}
	}

}
