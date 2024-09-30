using Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverGenericReducersWithActionInMethodSignatureTests.SupportFiles;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverGenericReducersWithActionInMethodSignatureTests;

public class DiscoverGenericReducersWithActionInMethodSignatureTests : IAsyncLifetime
{
	private readonly IServiceProvider ServiceProvider;
	private readonly IDispatcher Dispatcher;
	private readonly IStore Store;
	private readonly IState<TestState<char>> State;

	[Fact]
	public void WhenActionIsDispatched_ThenReducerWithActionInMethodSignatureIsExecuted()
	{
		Assert.Equal(0, State.Value.Counters['A']);
		Assert.Equal(0, State.Value.Counters['B']);
		Dispatcher.Dispatch(new IncrementItemAction<char>('A'));
		// 2 Reducers
		// 1 assembly scanned (generic descendant)
		// + 1 type scanned (closed generic)
		Assert.Equal(2, State.Value.Counters['A']);
		Assert.Equal(0, State.Value.Counters['B']);
	}

	public DiscoverGenericReducersWithActionInMethodSignatureTests()
	{
		var services = new ServiceCollection();
		services.AddFluxor(x => x
			.ScanAssemblies(GetType().Assembly)
			.ScanTypes(typeof(OpenGenericReducers<char>))
			.AddMiddleware<IsolatedTests>());

		ServiceProvider = services.BuildServiceProvider();
		Dispatcher = ServiceProvider.GetService<IDispatcher>();
		Store = ServiceProvider.GetRequiredService<IStore>();
		State = ServiceProvider.GetRequiredService<IState<TestState<char>>>();
	}

	async Task IAsyncLifetime.InitializeAsync() =>
		await Store.InitializeAsync();

	Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;
}
