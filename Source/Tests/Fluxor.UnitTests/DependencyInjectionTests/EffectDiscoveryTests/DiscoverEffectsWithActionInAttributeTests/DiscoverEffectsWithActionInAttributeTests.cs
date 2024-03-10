using Fluxor.UnitTests.DependencyInjectionTests.EffectDiscoveryTests.DiscoverEffectsWithActionInAttributeTests.SupportFiles;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.UnitTests.DependencyInjectionTests.EffectDiscoveryTests.DiscoverEffectsWithActionInAttributeTests;

public class DiscoverEffectsWithActionInAttributeTests : IAsyncLifetime
{
	private readonly IServiceProvider ServiceProvider;
	private readonly IDispatcher Dispatcher;
	private readonly IStore Store;
	private readonly IState<TestState> State;

	[Fact]
	public void WhenActionIsDispatched_ThenEffectWithActionInMethodSignatureIsExecuted()
	{
		Dispatcher.Dispatch(new TestAction());
		// 4 effects.
		// Static & Instance
		// 2 assembly scanned
		// + 2 type scanned
		Assert.Equal(4, State.Value.Count);
	}


	public DiscoverEffectsWithActionInAttributeTests()
	{
		var services = new ServiceCollection();
		services.AddFluxor(x => x
			.ScanAssemblies(GetType().Assembly)
			.ScanTypes(
				typeof(TypesThatShouldOnlyBeScannedExplicitly.ExplicitlyScannedInstanceTestEffects),
				typeof(TypesThatShouldOnlyBeScannedExplicitly.ExplicitlyScannedStaticTestEffects)
			)
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
