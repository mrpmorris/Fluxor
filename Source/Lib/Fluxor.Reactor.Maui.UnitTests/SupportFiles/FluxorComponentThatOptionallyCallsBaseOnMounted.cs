using Fluxor.Reactor.Maui.Components;
using MauiReactor;

namespace Fluxor.Reactor.Maui.UnitTests.SupportFiles;

public class FluxorComponentThatOptionallyCallsBaseOnMounted : FluxorComponent
{
	private bool CallBaseOnMounted;

	protected override void OnMounted()
	{
		if (CallBaseOnMounted)
			base.OnMounted();
	}

	public void Test_OnMounted(bool callBase)
	{
		CallBaseOnMounted = callBase;
		OnMounted();
	}

	public override VisualNode Render()
	{
		throw new System.NotImplementedException();
	}
}
