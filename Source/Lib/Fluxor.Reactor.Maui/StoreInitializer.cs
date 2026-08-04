using Fluxor.Reactor.Maui.Components;
using MauiReactor;
using System.Linq;

namespace Fluxor.Reactor.Maui;

/// <summary>
/// Initializes the store for the current user. This should be placed in the root app component.
/// </summary>
public partial class StoreInitializer : FluxorComponent
{
	[Inject]
	private IStore Store;

	public override VisualNode Render()
	{
		return Children().Single();
	}

	/// <summary>
	/// Initializes the store
	/// </summary>
	protected override void OnMounted()
	{
		Store.InitializeAsync();

		base.OnMounted();
	}
}
