﻿using Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverReducersWithActionInAttributeTests.SupportFiles;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverReducersWithActionInAttributeTests;

public class DiscoverReducersWithActionInAttributeTests : IAsyncLifetime
{
	private readonly IServiceProvider ServiceProvider;
	private readonly IDispatcher Dispatcher;
	private readonly IStore Store;
	private readonly IState<TestState> State;

	[Fact]
	public void WhenActionIsDispatched_ThenReducerWithActionInMethodSignatureIsExecuted()
	{
		Assert.Equal(0, State.Value.Counter);
		Dispatcher.Dispatch(new TestAction());
		// 2 Reducers
		// 1 assembly scanned (generic descendant)
		// + 1 type scanned (closed generic)
		Assert.Equal(2, State.Value.Counter);
	}

	public DiscoverReducersWithActionInAttributeTests()
	{
		var services = new ServiceCollection();
		services.AddFluxor(x => x
			.AddModule<GeneratedFluxorModule>()
			.ScanTypes(typeof(TypesThatShouldOnlyBeScannedExplicitly.ExplicitlyScannedReducers))
			.AddMiddleware<IsolatedTests>());

		ServiceProvider = services.BuildServiceProvider();
		Dispatcher = ServiceProvider.GetService<IDispatcher>();
		Store = ServiceProvider.GetRequiredService<IStore>();
		State = ServiceProvider.GetRequiredService<IState<TestState>>();
	}

	async Task IAsyncLifetime.InitializeAsync() =>
		await Store.InitializeAsync();

	Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;
}
