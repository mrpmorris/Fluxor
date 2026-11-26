using Fluxor.DependencyInjection;
using Fluxor.UnitTests.DependencyInjectionTests.ScanOptionsTests.SupportFiles.Hidden;
using Fluxor.UnitTests.DependencyInjectionTests.ScanOptionsTests.SupportFiles.Hidden2;
using Fluxor.UnitTests.DependencyInjectionTests.ScanOptionsTests.SupportFiles.HiddenSibling;
using Fluxor.UnitTests.DependencyInjectionTests.ScanOptionsTests.SupportFiles.Visible;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.UnitTests.DependencyInjectionTests.ScanOptionsTests;

public class ScanOptionsTests
{
	private static readonly Assembly TestAssembly = typeof(ScanOptionsTests).Assembly;
	private static readonly Assembly FluxorAssembly = typeof(IStore).Assembly;

	private static async Task<(IServiceProvider Services, IStore Store)> BuildAsync(Action<FluxorOptions> configure)
	{
		var services = new ServiceCollection();
		services.AddFluxor(configure);
		IServiceProvider serviceProvider = services.BuildServiceProvider();
		IStore store = serviceProvider.GetRequiredService<IStore>();
		await store.InitializeAsync();
		return (serviceProvider, store);
	}

	/// <summary>True when the visible test namespace was scanned, i.e. its effect is registered.</summary>
	private static bool VisibleNamespaceWasScanned(IServiceProvider services)
	{
		var action = new ProbeAction(Guid.NewGuid());
		services.GetRequiredService<IDispatcher>().Dispatch(action);
		return ProbeEffects.Seen.ContainsKey(action.Id);
	}

	private static Type[] FeatureStateTypes(IStore store) =>
		store.Features.Values.Select(x => x.GetStateType()).ToArray();

	[Fact]
	public async Task WhenAnAssemblyIsScanned_ThenNamespacesWithoutMiddlewareAreFoundAndOthersSkipped()
	{
		var (services, store) = await BuildAsync(x => x.ScanAssemblies(TestAssembly));

		Assert.True(VisibleNamespaceWasScanned(services));
		Assert.DoesNotContain(typeof(HiddenState), FeatureStateTypes(store));
	}

	[Fact]
	public async Task WhenAdditionalAssembliesArePassedToScanAssemblies_ThenTheyAreScanned()
	{
		var (services, _) = await BuildAsync(x => x.ScanAssemblies(FluxorAssembly, TestAssembly));

		Assert.True(VisibleNamespaceWasScanned(services));
	}

	[Fact]
	public async Task WhenScanAssembliesIsCalledTwice_ThenTheAssembliesFromBothCallsAreScanned()
	{
		var (services, _) = await BuildAsync(x => x
			.ScanAssemblies(TestAssembly)
			.ScanAssemblies(FluxorAssembly));

		Assert.True(VisibleNamespaceWasScanned(services));
	}

	[Fact]
	public async Task WhenAMiddlewareIsAdded_ThenItsNamespaceIsScannedButNotANamespaceSharingItsPrefix()
	{
		var (_, store) = await BuildAsync(x => x
			.ScanAssemblies(FluxorAssembly)
			.AddMiddleware<HiddenMarker>());

		Type[] stateTypes = FeatureStateTypes(store);
		Assert.Contains(typeof(HiddenState), stateTypes);
		Assert.DoesNotContain(typeof(HiddenSiblingState), stateTypes);
		Assert.DoesNotContain(typeof(Hidden2State), stateTypes);
	}

	[Fact]
	public async Task WhenTwoMiddlewaresAreAdded_ThenBothOfTheirNamespacesAreScanned()
	{
		var (_, store) = await BuildAsync(x => x
			.ScanAssemblies(FluxorAssembly)
			.AddMiddleware<HiddenMarker>()
			.AddMiddleware<Hidden2Marker>());

		Type[] stateTypes = FeatureStateTypes(store);
		Assert.Contains(typeof(HiddenState), stateTypes);
		Assert.Contains(typeof(Hidden2State), stateTypes);
	}

	[Fact]
	public async Task WhenAMiddlewareIsAdded_ThenTheStoreUsesIt()
	{
		var (_, store) = await BuildAsync(x => x.ScanAssemblies(FluxorAssembly).AddMiddleware<HiddenMarker>());

		Assert.IsType<HiddenMarker>(Assert.Single(store.GetMiddlewares()));
	}

	[Fact]
	public async Task WhenTheSameMiddlewareIsAddedTwice_ThenTheStoreUsesItOnce()
	{
		var (_, store) = await BuildAsync(x => x
			.ScanAssemblies(FluxorAssembly)
			.AddMiddleware<HiddenMarker>()
			.AddMiddleware<HiddenMarker>());

		Assert.IsType<HiddenMarker>(Assert.Single(store.GetMiddlewares()));
	}

	[Fact]
	public async Task WhenFluxorIsAdded_ThenIActionSubscriberIsTheStore()
	{
		var (services, store) = await BuildAsync(x => x.ScanAssemblies(FluxorAssembly).AddMiddleware<HiddenMarker>());

		Assert.Same(store, services.GetRequiredService<IActionSubscriber>());
	}

	[Fact]
	public async Task WhenFluxorIsAdded_ThenStateSelectionsCanBeResolved()
	{
		var (services, _) = await BuildAsync(x => x.ScanAssemblies(FluxorAssembly).AddMiddleware<HiddenMarker>());

		var selection = services.GetRequiredService<IStateSelection<HiddenState, int>>();
		selection.Select(x => x.Value);

		Assert.Equal(0, selection.Value);
	}
}
