using Fluxor.UnitTests.DependencyInjectionTests.InstanceMethodTests.SupportFiles;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.UnitTests.DependencyInjectionTests.InstanceMethodTests;

public class InstanceMethodTests : IAsyncLifetime
{
	private readonly IServiceProvider ServiceProvider;
	private readonly IDispatcher Dispatcher;
	private readonly IStore Store;
	private readonly IState<CounterState> State;
	private readonly EffectLog EffectLog;

	public InstanceMethodTests()
	{
		var services = new ServiceCollection();
		services.AddSingleton<EffectLog>();
		services.AddFluxor(x => x.ScanTypes(
			typeof(CounterState),
			typeof(InstanceReducers),
			typeof(InstanceEffects)));

		ServiceProvider = services.BuildServiceProvider();
		Dispatcher = ServiceProvider.GetRequiredService<IDispatcher>();
		Store = ServiceProvider.GetRequiredService<IStore>();
		State = ServiceProvider.GetRequiredService<IState<CounterState>>();
		EffectLog = ServiceProvider.GetRequiredService<EffectLog>();
	}

	[Fact]
	public void WhenAnInstanceReducerTakesTheAction_ThenItReducesUsingItsHostInstance()
	{
		Dispatcher.Dispatch(new AddAction(2));

		Assert.Equal(20, State.Value.Count);
	}

	[Fact]
	public void WhenAnInstanceReducerHasTheActionTypeInItsAttribute_ThenItReducesUsingItsHostInstance()
	{
		Dispatcher.Dispatch(new AddAction(2));
		Dispatcher.Dispatch(new ResetAction());

		Assert.Equal(-1, State.Value.Count);
	}

	[Fact]
	public void WhenInstanceEffectsReact_ThenTheyRunOnTheirInjectedHost()
	{
		Dispatcher.Dispatch(new AddAction(3));
		Dispatcher.Dispatch(new ResetAction());

		Assert.Equal(new[] { "OnAdd 3", "OnReset" }, EffectLog.Entries);
	}

	Task IAsyncLifetime.InitializeAsync() => Store.InitializeAsync();

	Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;
}
